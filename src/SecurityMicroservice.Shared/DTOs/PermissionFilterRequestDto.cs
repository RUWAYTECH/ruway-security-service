using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.DTOs;

public class PermissionFilterRequestDto: PaginationRequestDto
{
    public Guid RoleId { get; set; }
}