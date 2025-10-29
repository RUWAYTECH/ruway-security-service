using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.AUDITORIA;

public static class AuditoriaCoordinator
{
    public static async Task InitializeAsync(SecurityDbContext context, IServiceProvider serviceProvider)
    {
        Console.WriteLine("📦 Initializing AUDITORIA Application Data...");
        
        await AuditoriaApplicationSeedData.SeedAsync(context);
        await AuditoriaRolesSeedData.SeedAsync(context);
        await AuditoriaModulesAndOptionsSeedData.SeedAsync(context);
        await AuditoriaPermissionsSeedData.SeedAsync(context);
        await SeedClientApiAuditoria.InitializeAsync(serviceProvider);
        Console.WriteLine("✅ AUDITORIA Application Data initialized successfully!");
    }
}