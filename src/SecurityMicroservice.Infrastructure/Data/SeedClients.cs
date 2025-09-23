using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using SecurityMicroservice.Infrastructure.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityMicroservice.Infrastructure.Data;

public static class SeedClients
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // Create scopes first
        var apiScope = await scopeManager.FindByNameAsync("rokys-memo-api");
        if (apiScope == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "rokys-memo-api",
                DisplayName = "Rokys Memo API Access",
                Description = "Access to Rokys Memo API",
                Resources = { "rokys-memo-api" }
            });
        }

        // Delete existing client if it exists (to update configuration)
        var existingClient = await applicationManager.FindByClientIdAsync("rokys-memo-api");
        if (existingClient != null)
        {
            await applicationManager.DeleteAsync(existingClient);
        }

        // Create the rokys-memo-api client with updated configuration
        var clientDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "rokys-memo-api",
            ClientSecret = "rokys-memo-secret",
            DisplayName = "Rokys Memo API",
            Type = ClientTypes.Confidential,
            ConsentType = ConsentTypes.Implicit,
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Introspection,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.GrantTypes.Password,
                Permissions.GrantTypes.RefreshToken,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + "rokys-memo-api"
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        await applicationManager.CreateAsync(clientDescriptor);
    }
}