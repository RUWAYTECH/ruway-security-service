using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.Request.UserPermission;

public class UserPermissionPaginationRequestDto : PaginationRequestDto
{
    public Guid? UserId { get; set; }
    public Guid? PermissionId { get; set; }
    public Guid? ApplicationId { get; set; }
    public string? ApplicationCode { get; set; }
    public bool? IsActive { get; set; }
    public bool? IncludeExpired { get; set; }
}