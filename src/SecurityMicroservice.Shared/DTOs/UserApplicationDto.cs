namespace SecurityMicroservice.Shared.DTOs;

public class UserApplicationDto
{
    public Guid UserId { get; set; }
    public Guid ApplicationId { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties for display
    public string Username { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationCode { get; set; } = string.Empty;
}