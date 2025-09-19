
using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class Module : BaseEntity
{
    public Guid ModuleId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Order { get; set; }
    public Guid ApplicationId { get; set; }
    
    // Navigation properties
    public Application Application { get; set; } = null!;
    public ICollection<Option> Options { get; set; } = new List<Option>();
}