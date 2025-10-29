using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.MEMOS;

public static class MemosCoordinator
{
    public static async Task InitializeAsync(SecurityDbContext context, IServiceProvider serviceProvider)
    {
        Console.WriteLine("📦 Initializing MEMOS Application Data...");
        
        await MemosApplicationSeedData.SeedAsync(context);
        await MemosRolesSeedData.SeedAsync(context);
        await MemosModulesAndOptionsSeedData.SeedAsync(context);
        await MemosPermissionsSeedData.SeedAsync(context);
        await SeedClientApiMemos.InitializeAsync(serviceProvider);
        
        Console.WriteLine("✅ MEMOS Application Data initialized successfully!");
    }
}