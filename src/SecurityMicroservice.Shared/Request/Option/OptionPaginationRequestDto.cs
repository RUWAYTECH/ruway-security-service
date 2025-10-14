using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.Request.Option;

public class OptionPaginationRequestDto : PaginationRequestDto
{
    public Guid? ModuleId { get; set; }
    public string? ApplicationCode { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? HttpMethod { get; set; }
    public bool? IsActive { get; set; }
}