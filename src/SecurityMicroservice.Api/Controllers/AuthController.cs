using Azure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Response.Common;
using System.Collections.Immutable;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static SecurityMicroservice.Domain.Constants.Constants;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("connect")]
public class AuthController : ControllerBase
{
    private readonly Application.Services.IAuthenticationService _authenticationService;
    private readonly Application.IServices.IRecaptchaService _recaptchaService;
    private readonly IConfiguration _configuration;

    public AuthController(Application.Services.IAuthenticationService authenticationService, Application.IServices.IRecaptchaService recaptchaService, IConfiguration configuration)
    {
        _authenticationService = authenticationService;
        _recaptchaService = recaptchaService;
        _configuration = configuration;
    }

    [HttpPost("token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()  ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        bool useRecaptcha = Convert.ToBoolean(_configuration["Recaptcha:UseRecaptcha"]);
        var recaptchaToken = (string?)request.GetParameter("recaptchaToken");
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrEmpty(request.GrantType))
        {
            throw new InvalidOperationException("The grant_type parameter is missing.");
        }

        if (request.IsPasswordGrantType())
        {
            if (useRecaptcha)
            {
                if (recaptchaToken != null)
                {
                    var (isValid, score) = await _recaptchaService.ValidateTokenAsync(recaptchaToken, clientIp);
                    if (!isValid)
                    {
                        return Forbid(
                            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                            properties: new(new Dictionary<string, string?>
                            {
                                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Validación de reCAPTCHA fallido."
                            }));
                    }
                }
                else
                {
                    return Forbid(
                            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                            properties: new(new Dictionary<string, string?>
                            {
                                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Es requerido que se envie el token de reCAPTCHA."
                            }));
                }
            }

            var user = await _authenticationService.ValidateUserAsync(request.Username!, request.Password!);
            if (user == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Usuario o contraseña invalidas."
                    }));
            }

            var tokenResponse = await _authenticationService.GenerateTokenResponseAsync(user);
            
            // Create claims identity using helper method
            var identity = CreateUserIdentity(user, tokenResponse);
            identity.SetScopes(tokenResponse.Scope.Split(' ').ToImmutableArray());
            identity.SetDestinations(GetDestinations);

            await _authenticationService.UpdateLastLoginAsync(user.UserId);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (request.IsClientCredentialsGrantType())
        {
            // Client Credentials flow - for machine-to-machine communication
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Use the client_id as the subject for client credentials
            identity.SetClaim(Claims.Subject, request.ClientId!)
                   .SetClaim(Claims.Name, request.ClientId!);

            // Set default scopes for client credentials
            identity.SetScopes(new[] { "api" }.ToImmutableArray());

            // For client credentials, we can set the client_id as resource
            // This allows the client to access resources with its own identity
            identity.SetResources(request.ClientId!);

            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            return await HandleRefreshTokenAsync();
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authenticationService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authenticationService.ResetPasswordAsync(request);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// OAuth 2.0 Authorization endpoint (GET) - Initiates authorization flow
    /// Handles authorization requests and validates OAuth parameters
    /// </summary>
    [HttpGet("authorize")]
    public IActionResult Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        
        // Validate the authorization request
        if (string.IsNullOrEmpty(request.ClientId))
        {
            return BadRequest(new
            {
                error = Errors.InvalidRequest,
                error_description = "The 'client_id' parameter is missing."
            });
        }

        if (string.IsNullOrEmpty(request.ResponseType))
        {
            return BadRequest(new
            {
                error = Errors.InvalidRequest,
                error_description = "The 'response_type' parameter is missing."
            });
        }

        // Validate response_type (should be 'code' for authorization code flow)
        if (!request.ResponseType.Equals("code", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                error = Errors.UnsupportedResponseType,
                error_description = "Only 'code' response type is supported."
            });
        }

        // Check if user is authenticated
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            // User is not authenticated, initiate authentication
            return Challenge();
        }

        // User is authenticated, show consent page or auto-approve
        // For simplicity, we'll auto-approve if user is authenticated
        // In production, you'd typically show a consent screen

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     User.FindFirst(Claims.Subject)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        // Create authorization code identity
        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, userId);

        // Set requested scopes (or default scopes)
        var scopes = request.GetScopes();
        if (!scopes.Any())
        {
            scopes = ImmutableArray.Create(Scopes.OpenId, "security", "auditoria", "memos");
        }
        identity.SetScopes(scopes);

        // Set resources
        identity.SetResources("security_api");

        // Set destinations for authorization code
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// OAuth 2.0 Authorization endpoint (POST) - Processes user consent
    /// Handles user authorization decisions (approve/deny)
    /// </summary>
    [HttpPost("authorize")]
    public IActionResult Accept([FromForm] string? submit)
    {
        var request = HttpContext.GetOpenIddictServerRequest()  ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Check if user denied the authorization
        if (string.Equals(submit, "deny", StringComparison.InvariantCultureIgnoreCase))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = 
                        "The authorization was denied by the user."
                }));
        }

        // Ensure user is authenticated
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Challenge();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     User.FindFirst(Claims.Subject)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        // User approved the authorization, create authorization code
        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, userId);

        // Set approved scopes
        var scopes = request.GetScopes();
        if (!scopes.Any())
        {
            scopes = ImmutableArray.Create(Scopes.OpenId, "security", "auditoria", "memos");
        }
        identity.SetScopes(scopes);

        // Set resources
        identity.SetResources("security_api");

        // Set destinations
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // TODO: Implement logout logic
        await HttpContext.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        return Ok(new { message = "Logged out successfully" });
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (claim.Subject!.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;

                yield break;

            case "roles":
            case "permissions":
            case "employee_id":
            case "first_name":
            case "last_name":
                yield return Destinations.AccessToken;
                yield break;

            case Claims.Subject:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }

    /// <summary>
    /// Handles refresh token grant type requests
    /// </summary>
    /// <returns>SignInResult with new tokens or Forbid result if validation fails</returns>
    private async Task<IActionResult> HandleRefreshTokenAsync()
    {
        // Authenticate using the refresh token
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        
        if (!result.Succeeded)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "El refresh token es inválido o ha expirado."
                }));
        }

        // Extract and validate user ID from the refresh token
        var userId = result.Principal?.GetClaim(Claims.Subject);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "El usuario asociado con el refresh token no puede ser encontrado."
                }));
        }

        // Validate user still exists and is active using application service
        var user = await _authenticationService.GetUserByIdAsync(userGuid);
        if (user == null || user.Status != UserStatus.Active)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "La cuenta de usuario ya no es válida."
                }));
        }

        // Generate fresh token response with updated user data
        var tokenResponse = await _authenticationService.GenerateTokenResponseAsync(user);
        
        // Create new identity with fresh claims
        var identity = CreateUserIdentity(user, tokenResponse);

        // Restore the scopes and resources from the original refresh token
        if (result.Principal != null)
        {
            identity.SetScopes(result.Principal.GetScopes());
            
            var resources = result.Principal.GetResources();
            if (resources.Any())
            {
                identity.SetResources(resources);
            }
        }

        // Set the destinations for all claims using the existing GetDestinations method
        identity.SetDestinations(GetDestinations);

        var principal = new ClaimsPrincipal(identity);

        // Return the sign-in result with the correct scheme
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Creates a user identity with all necessary claims
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="tokenResponse">Token response with roles and permissions</param>
    /// <returns>ClaimsIdentity with user claims</returns>
    private static ClaimsIdentity CreateUserIdentity(User user, TokenResponse tokenResponse)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, user.UserId.ToString())
                .SetClaim(Claims.Name, user.UserName)
                .SetClaim("employee_id", user.EmployeeId?.ToString())
                .SetClaim("first_name", user.FirstName)
                .SetClaim("last_name", user.LastName)
                .SetClaim("date_of_birth", user.DateOfBirth?.ToString("yyyy-MM-dd"))
                .SetClaim(Claims.Email, user.Email);

        // Set roles - force array by ensuring at least 2 claims if needed
        if (tokenResponse.Roles?.Any() == true)
        {
            if (tokenResponse.Roles.Count == 1)
            {
                // For single role, add a marker to force array serialization
                var rolesWithMarker = new List<string>(tokenResponse.Roles)
                {
                    "FORCE:ARRAY:FORCE" // Client should filter this out
                };
                identity.SetClaims(ClaimNames.Roles, rolesWithMarker.ToImmutableArray());

                // For single role, duplicate it to force array serialization
                var role = tokenResponse.Roles[0];
                identity.SetClaims(ClaimNames.Roles, new[] { role, role }.ToImmutableArray());
            }
            else
            {
                identity.SetClaims(ClaimNames.Roles, tokenResponse.Roles.ToImmutableArray());
            }
        }
        else
        {
            identity.SetClaims(ClaimNames.Roles, ImmutableArray<string>.Empty);
        }

        // Set permissions - force array by adding marker for single elements
        if (tokenResponse.Permissions?.Any() == true)
        {
            if (tokenResponse.Permissions.Count == 1)
            {
                // For single permission, add a marker to force array serialization
                var permissionsWithMarker = new List<string>(tokenResponse.Permissions)
                {
                    "FORCE:ARRAY:FORCE" // Client should filter this out
                };
                identity.SetClaims(ClaimNames.Permissions, permissionsWithMarker.ToImmutableArray());
            }
            else
            {
                identity.SetClaims(ClaimNames.Permissions, tokenResponse.Permissions.ToImmutableArray());
            }
        }
        else
        {
            identity.SetClaims(ClaimNames.Permissions, ImmutableArray<string>.Empty);
        }

        return identity;
    }

    /// <summary>
    /// Creates a standardized error response for refresh token failures
    /// </summary>
    /// <param name="description">Error description</param>
    /// <returns>Forbid result with error details</returns>
    private IActionResult CreateRefreshTokenError(string description)
    {
        return Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }));
    }
}