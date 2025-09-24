using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SecurityMicroservice.Infrastructure.Data;
public static class SeedAudit
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // Create scopes first
        var apiScope = await scopeManager.FindByNameAsync("rokys-audit-api");
        if (apiScope == null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "rokys-audit-api",
                DisplayName = "Rokys Audit API Access",
                Description = "Access to Rokys Audit API",
                Resources = { "rokys-audit-api" }
            });
        }

        // Delete existing client if it exists (to update configuration)
        var existingClient = await applicationManager.FindByClientIdAsync("rokys-audit-api");
        if (existingClient != null)
        {
            await applicationManager.DeleteAsync(existingClient);
        }

        var clientDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "rokys-audit-api",
            ClientSecret = "rokys-audit-secret",
            DisplayName = "Rokys Audit API",
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
                Permissions.Prefixes.Scope + "rokys-audit-api"
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        await applicationManager.CreateAsync(clientDescriptor);
    }
}
