using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.DTOs;

public class OptionDto : AuditEntityDto
{
    public Guid OptionId { get; set; }
    public Guid ModuleId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public string ModuleName { get; set; } = string.Empty;
    public string ApplicationCode { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
}

