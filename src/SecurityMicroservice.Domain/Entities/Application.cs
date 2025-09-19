using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class Application: BaseEntity
{
    public Guid ApplicationId { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty; // e.g., AUDITORIA, MEMOS
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<Module> Modules { get; set; } = new List<Module>();
}