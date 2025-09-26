using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.Request.Permission;
using SecurityMicroservice.Shared.Response.Common;
using SecurityMicroservice.Shared.Response.Permission;

namespace SecurityMicroservice.Application.IServices
{
    public interface IPermissionService : IBaseService<PermissionRequestDto, PermissionResponseDto>
    {
        Task<ResponseDto<PaginationResponseDto<PermissionResponseDto>>> GetPaged(PaginationRequestDto requestDto);
    }
}
