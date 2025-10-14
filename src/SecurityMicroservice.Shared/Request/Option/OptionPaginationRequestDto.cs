using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.Request.Option;

public class OptionPaginationRequestDto : PaginationRequestDto
{
    public Guid ModuleId { get; set; }
}