using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.User;
using SecurityMicroservice.Shared.Response.Common;
using SecurityMicroservice.Shared.Response.User;

namespace SecurityMicroservice.Application.IServices
{
    public interface IUserService : IBaseService<UserRequestDto, UserResponseDto>
    {
        Task<ResponseDto<PaginationResponseDto<UserResponseDto>>> GetPaged(UserPaginationRequestDto requestDto);
    }
}
