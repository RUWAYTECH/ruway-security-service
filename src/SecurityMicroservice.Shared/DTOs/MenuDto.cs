namespace SecurityMicroservice.Shared.DTOs;

public class MenuDto
{
    public string ApplicationUrl { get; set; } = string.Empty;
    public string ApplicationCode { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationIcon { get; set; } = string.Empty;
    public List<ModuleDto> Modules { get; set; } = new();
}

public class ModuleDto
{
    public Guid ModuleId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<MenuOptionDto> Options { get; set; } = new();
}

public class MenuOptionDto
{
    public Guid OptionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<string> AllowedActions { get; set; } = new();
}