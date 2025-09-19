namespace SecurityMicroservice.Shared.DTOs;

public class OptionDto
{
    public Guid OptionId { get; set; }
    public Guid ApplicationId { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOptionRequest
{
    public Guid ApplicationId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
}

public class UpdateOptionRequest
{
    public string? Module { get; set; }
    public string? Name { get; set; }
    public string? Route { get; set; }
    public string? HttpMethod { get; set; }
    public bool? IsActive { get; set; }
}