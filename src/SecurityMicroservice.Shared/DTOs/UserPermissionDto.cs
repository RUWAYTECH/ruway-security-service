namespace SecurityMicroservice.Shared.DTOs;

public class UserPermissionDto
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? GrantedByUserId { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties for display
    public string Username { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationCode { get; set; } = string.Empty;
    public string ActionCode { get; set; } = string.Empty;
    public string GrantedByUsername { get; set; } = string.Empty;
}