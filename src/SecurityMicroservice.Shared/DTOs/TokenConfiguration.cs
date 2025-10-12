namespace SecurityMicroservice.Shared.DTOs;

public class TokenConfiguration
{
    public int AccessTokenLifetimeMinutes { get; set; } = 60; // Default: 1 hour
    public int RefreshTokenLifetimeDays { get; set; } = 14; // Default: 14 days
    public int IdentityTokenLifetimeMinutes { get; set; } = 5; // Default: 5 minutes
}