using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.Request.UserApplication;

public class UserApplicationPaginationRequestDto : PaginationRequestDto
{
    public Guid? UserId { get; set; }
    public Guid? ApplicationId { get; set; }
    public string? ApplicationCode { get; set; }
    public bool? IsActive { get; set; }
}