namespace SecurityMicroservice.Shared.Common
{
    public class PaginationRequestDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? Filter { get; set; }
        public string? SortBy { get; set; }
    }
}
