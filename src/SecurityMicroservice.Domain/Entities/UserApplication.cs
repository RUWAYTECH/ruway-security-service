using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class UserApplication: BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ApplicationId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? Notes { get; set; } = string.Empty;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Application Application { get; set; } = null!;
}