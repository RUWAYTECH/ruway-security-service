using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Core;

/// <summary>
/// OpenIddict core configuration seed data
/// Contains basic OAuth2/OIDC scopes and applications that are reusable across clients
/// </summary>
public static class OpenIddictCoreSeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();

        Console.WriteLine("🚀 Initializing OpenIddict Core Configuration...");

        // === CORE SCOPES ===
        var coreScopes = new[]
        {
            new { Name = "security", DisplayName = "Security API", Description = "Access to security microservice API" },
            new { Name = "openid", DisplayName = "OpenID", Description = "OpenID Connect scope" },
            new { Name = "profile", DisplayName = "Profile", Description = "Access to user profile information" },
            new { Name = "email", DisplayName = "Email", Description = "Access to user email address" }
        };

        foreach (var scopeInfo in coreScopes)
        {
            if (await scopeManager.FindByNameAsync(scopeInfo.Name) == null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = scopeInfo.Name,
                    DisplayName = scopeInfo.DisplayName,
                    Description = scopeInfo.Description
                });
                Console.WriteLine($"   ✅ Scope '{scopeInfo.Name}' created");
            }
            else
            {
                Console.WriteLine($"   ℹ️ Scope '{scopeInfo.Name}' already exists");
            }
        }

        // === CORE OAUTH2 APPLICATIONS ===
        var coreApplications = new[]
        {
            new 
            { 
                ClientId = "security-api-client",
                DisplayName = "Security API Client",
                Permissions = new[]
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Introspection,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "security"
                }
            }
        };

        foreach (var appInfo in coreApplications)
        {
            if (await manager.FindByClientIdAsync(appInfo.ClientId) == null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    Type = OpenIddictConstants.ClientTypes.Public,
                    ClientId = appInfo.ClientId,
                    DisplayName = appInfo.DisplayName,
                    ConsentType = OpenIddictConstants.ConsentTypes.Implicit
                };
                
                foreach (var permission in appInfo.Permissions)
                {
                    descriptor.Permissions.Add(permission);
                }
                
                await manager.CreateAsync(descriptor);
                Console.WriteLine($"   ✅ OAuth2 Application '{appInfo.ClientId}' created");
            }
            else
            {
                Console.WriteLine($"   ℹ️ OAuth2 Application '{appInfo.ClientId}' already exists");
            }
        }

        Console.WriteLine("✅ OpenIddict Core Configuration completed successfully");
    }
}