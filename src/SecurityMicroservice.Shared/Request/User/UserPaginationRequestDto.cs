using SecurityMicroservice.Shared.Common;

namespace SecurityMicroservice.Shared.Request.User
{
    public class UserPaginationRequestDto : PaginationRequestDto
    {
        public string ApplicationCode { get; set; }
    }
}
