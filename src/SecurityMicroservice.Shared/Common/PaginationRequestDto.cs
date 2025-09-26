namespace SecurityMicroservice.Shared.Common
{
    public class PaginationRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Filter { get; set; }
        public string? SortBy { get; set; }
    }
}
