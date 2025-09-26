namespace SecurityMicroservice.Shared.DTOs;

public class RoleDto
{
    public Guid RoleId { get; set; }
    public Guid ApplicationId { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    //public string? Url { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class CreateRoleRequest
{
    public Guid ApplicationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    //public string? Url { get; set; }
}

public class UpdateRoleRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    //public string? Url { get; set; }
    public bool? IsActive { get; set; }
}