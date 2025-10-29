using SecurityMicroservice.Infrastructure.Data.Seeds.Core;
using SecurityMicroservice.Infrastructure.Data.Seeds.Clients.MEMOS;
using SecurityMicroservice.Infrastructure.Data.Seeds.Clients.AUDITORIA;
using SecurityMicroservice.Infrastructure.Services;

namespace SecurityMicroservice.Infrastructure.Data.Seeds;

/// <summary>
/// Master seed coordinator that orchestrates the initialization of all seed data
/// Follows the principle of Core Security System first, then Client-specific data
/// </summary>
public static class MasterSeedCoordinator
{
    /// <summary>
    /// Initializes all seed data in the correct order
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="serviceProvider">Service provider for OpenIddict managers</param>
    /// <param name="passwordService">Password hashing service</param>
    /// <param name="enableClientSeeds">Whether to initialize client-specific seeds</param>
    public static async Task InitializeAllAsync(
        SecurityDbContext context,
        IServiceProvider serviceProvider,
        IPasswordService passwordService,
        bool enableClientSeeds = true)
    {
        Console.WriteLine("🎯 Starting Master Seed Initialization...");
        Console.WriteLine("========================================");

        try
        {
            // === PHASE 1: CORE SECURITY SYSTEM ===
            Console.WriteLine("\n📁 PHASE 1: Core Security System");
            Console.WriteLine("----------------------------------");

            await CoreSecuritySeedData.InitializeAsync(context, passwordService);
            await OpenIddictCoreSeedData.InitializeAsync(serviceProvider);

            // === PHASE 2: CLIENT-SPECIFIC DATA ===
            if (enableClientSeeds)
            {
                Console.WriteLine("\n📁 PHASE 2: Client-Specific Data");
                Console.WriteLine("---------------------------------");

                await MemosCoordinator.InitializeAsync(context, serviceProvider);
                await AuditoriaCoordinator.InitializeAsync(context, serviceProvider);
            }
            else
            {
                Console.WriteLine("\n⏭️ PHASE 2: Client seeds disabled");
            }

            Console.WriteLine("\n🎉 Master Seed Initialization completed successfully!");
            Console.WriteLine("===================================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error during seed initialization: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Initializes only core security system (useful for new client setups)
    /// </summary>
    public static async Task InitializeCoreOnlyAsync(
        SecurityDbContext context,
        IServiceProvider serviceProvider,
        IPasswordService passwordService)
    {
        await InitializeAllAsync(context, serviceProvider, passwordService, enableClientSeeds: false);
    }

}