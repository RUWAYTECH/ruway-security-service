using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class Option: BaseEntity
{
    public Guid OptionId { get; set; } = Guid.NewGuid();
    public Guid ModuleId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;    
    // Navigation properties
    public Module Module { get; set; } = null!;
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}