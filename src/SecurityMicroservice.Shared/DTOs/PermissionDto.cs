namespace SecurityMicroservice.Shared.DTOs;

public class PermissionDto
{
    public Guid PermissionId { get; set; }
    public Guid RoleId { get; set; }
    public Guid OptionId { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public string OptionCode { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public string OptionRoute { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}