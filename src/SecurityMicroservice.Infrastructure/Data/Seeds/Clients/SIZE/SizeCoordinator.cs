using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.SIZE;

public static class SizeCoordinator
{
    public static async Task InitializeAsync(SecurityDbContext context, IServiceProvider serviceProvider)
    {
        Console.WriteLine("📦 Initializing SIZE Application Data...");
        
        await SizeApplicationSeedData.SeedAsync(context);
        await SizeRolesSeedData.SeedAsync(context);
        await SizeModulesAndOptionsSeedData.SeedAsync(context);
        await SizePermissionsSeedData.SeedAsync(context);
        await SeedClientApiSize.InitializeAsync(serviceProvider);
        
        Console.WriteLine("✅ SIZE Application Data initialized successfully!");
    }
}
