using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Shared.DTOs;
using System.Collections.Immutable;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("connect")]
public class AuthController : ControllerBase
{
    private readonly Application.Services.IAuthenticationService _authenticationService;

    public AuthController(Application.Services.IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("token")]
    public async Task<IActionResult> Exchange()
    {
        // Parse OpenID Connect request manually from form data
        var request = new OpenIddictRequest();
        
        if (Request.HasFormContentType)
        {
            foreach (var parameter in Request.Form)
            {
                request.SetParameter(parameter.Key, parameter.Value.ToString());
            }
        }
        
        if (string.IsNullOrEmpty(request.GrantType))
        {
            throw new InvalidOperationException("The grant_type parameter is missing.");
        }

        if (request.IsPasswordGrantType())
        {
            var user = await _authenticationService.ValidateUserAsync(request.Username!, request.Password!);
            if (user == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid username or password."
                    }));
            }

            var tokenResponse = await _authenticationService.GenerateTokenResponseAsync(user);
            
            // Create claims identity
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, user.UserId.ToString())
                    .SetClaim(Claims.Name, user.Username)
                    .SetClaim("employee_id", user.EmployeeId?.ToString())
                    .SetClaims("roles", tokenResponse.Roles.ToImmutableArray())
                    .SetClaims("permissions", tokenResponse.Permissions.ToImmutableArray());

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
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            
            var userId = result.Principal?.GetClaim(Claims.Subject);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid refresh token."
                    }));
            }

            // Refresh user data and generate new token
            var user = await _authenticationService.ValidateUserAsync(result.Principal!.GetClaim(Claims.Name)!, "");
            if (user == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "User no longer valid."
                    }));
            }

            var tokenResponse = await _authenticationService.GenerateTokenResponseAsync(user);
            
            // Update claims with fresh data
            var identity = new ClaimsIdentity(result.Principal!.Identity);
            identity.SetClaim("employee_id", user.EmployeeId?.ToString())
                   .SetClaims("roles", tokenResponse.Roles.ToImmutableArray())
                   .SetClaims("permissions", tokenResponse.Permissions.ToImmutableArray());

            identity.SetScopes(tokenResponse.Scope.Split(' ').ToImmutableArray());
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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
        // Parse OpenID Connect request manually from query parameters
        var request = new OpenIddictRequest();
        
        foreach (var parameter in Request.Query)
        {
            request.SetParameter(parameter.Key, parameter.Value.ToString());
        }

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
        // Parse OpenID Connect request manually from form data
        var request = new OpenIddictRequest();
        
        if (Request.HasFormContentType)
        {
            foreach (var parameter in Request.Form)
            {
                request.SetParameter(parameter.Key, parameter.Value.ToString());
            }
        }

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
}