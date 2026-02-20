using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.SIZE;

public static class SeedClientApiSize
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // Create scopes first
        var apiScope = await scopeManager.FindByNameAsync("size-api");
        if (apiScope == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "size-api",
                DisplayName = "Size API Access",
                Description = "Access to Size API",
                Resources = { "size-api" }
            });
        }

        // Delete existing client if it exists (to update configuration)
        var existingClient = await applicationManager.FindByClientIdAsync("size-api");
        if (existingClient != null)
        {
            await applicationManager.DeleteAsync(existingClient);
        }

        var clientDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "size-api",
            ClientSecret = "size-secret",
            DisplayName = "Size API",
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
                Permissions.Prefixes.Scope + "size-api"
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        await applicationManager.CreateAsync(clientDescriptor);
    }
}
