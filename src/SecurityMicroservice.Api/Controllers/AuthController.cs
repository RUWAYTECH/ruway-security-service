using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
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