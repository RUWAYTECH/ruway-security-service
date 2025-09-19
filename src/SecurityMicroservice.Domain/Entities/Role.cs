using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class Role: BaseEntity
{
    public Guid RoleId { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Application Application { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}