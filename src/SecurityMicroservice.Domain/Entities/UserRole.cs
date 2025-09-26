using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class UserRole: BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? Notes { get; set; } = string.Empty;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}