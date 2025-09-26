using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.Request.UserRole;

public class UserRolePaginationRequestDto : PaginationRequestDto
{
    public Guid? UserId { get; set; }
    public Guid? RoleId { get; set; }
    public Guid? ApplicationId { get; set; }
    public string? ApplicationCode { get; set; }
    public bool? IsActive { get; set; }
}