namespace SecurityMicroservice.Infrastructure.Data.Seeds;

/// <summary>
/// Configuration settings for seed data initialization
/// Allows granular control over which seeds to run
/// </summary>
public class SeedConfiguration
{
    /// <summary>
    /// Enable/disable all seed operations
    /// </summary>
    public bool EnableSeeds { get; set; } = true;

    /// <summary>
    /// Enable/disable core security system seeds
    /// Should always be true for new deployments
    /// </summary>
    public bool EnableCoreSeeds { get; set; } = true;

    /// <summary>
    /// Enable/disable client-specific seeds
    /// Set to false when deploying for a new client
    /// </summary>
    public bool EnableClientSeeds { get; set; } = true;

    /// <summary>
    /// Enable/disable Ruway-specific seeds
    /// </summary>
    public bool EnableRuwaySeeds { get; set; } = true;

    /// <summary>
    /// Client name for logging and identification
    /// </summary>
    public string ClientName { get; set; } = "Ruway";

    /// <summary>
    /// Environment-specific seed behavior
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Whether to skip seeds if data already exists
    /// </summary>
    public bool SkipIfDataExists { get; set; } = true;

    /// <summary>
    /// Validate configuration settings
    /// </summary>
    public bool IsValid()
    {
        if (!EnableSeeds) return true; // If seeds are disabled, config is valid
        
        return EnableCoreSeeds; // Core seeds must be enabled if seeds are enabled
    }

    /// <summary>
    /// Get configuration summary for logging
    /// </summary>
    public string GetSummary()
    {
        return $"Seeds: {(EnableSeeds ? "Enabled" : "Disabled")}, " +
               $"Core: {(EnableCoreSeeds ? "Enabled" : "Disabled")}, " +
               $"Client: {(EnableClientSeeds ? "Enabled" : "Disabled")}, " +
               $"Ruway: {(EnableRuwaySeeds ? "Enabled" : "Disabled")}, " +
               $"Environment: {Environment}, " +
               $"Client: {ClientName}";
    }
}