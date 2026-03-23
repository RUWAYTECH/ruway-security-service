using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.DTOs;

public class RoleFilterRequestDto : PaginationRequestDto
{
    public Guid ApplicationId { get; set; }
    public string? ApplicationCode { get; set; }
    public string? ListInactive { get; set; }
}