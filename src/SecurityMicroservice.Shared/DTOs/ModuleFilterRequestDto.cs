using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.DTOs;

public class ModuleFilterRequestDto: PaginationRequestDto
{
    public Guid ApplicationId { get; set; }
}