namespace SecurityMicroservice.Shared.DTOs;

// DTO principal para operaciones CRUD de Module
public class ModuleManagementDto
{
    public Guid ModuleId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Order { get; set; }
    public Guid ApplicationId { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OptionDto> Options { get; set; } = new();
}

public class CreateModuleRequest
{
    public Guid ApplicationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class UpdateModuleRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int? Order { get; set; }
    public bool? IsActive { get; set; }
}