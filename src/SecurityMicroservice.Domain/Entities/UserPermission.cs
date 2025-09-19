using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class UserPermission: BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}