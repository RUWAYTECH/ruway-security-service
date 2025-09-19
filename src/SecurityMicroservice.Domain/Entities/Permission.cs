using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class Permission: BaseEntity
{
    public Guid PermissionId { get; set; } = Guid.NewGuid();
    public Guid RoleId { get; set; }
    public Guid OptionId { get; set; }
    public string ActionCode { get; set; } = string.Empty; // READ, CREATE, UPDATE, DELETE, APPROVE, EXPORT
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public Option Option { get; set; } = null!;
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}