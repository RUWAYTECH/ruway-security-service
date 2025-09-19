namespace SecurityMicroservice.Shared.DTOs;

public class PermissionDto
{
    public Guid PermissionId { get; set; }
    public Guid RoleId { get; set; }
    public Guid OptionId { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
    public string OptionRoute { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePermissionRequest
{
    public Guid RoleId { get; set; }
    public Guid OptionId { get; set; }
    public string ActionCode { get; set; } = string.Empty;
}

public class UpdatePermissionRequest
{
    public string? ActionCode { get; set; }
    public bool? IsActive { get; set; }
}