using SecurityMicroservice.Shared.Request.Permission;
using SecurityMicroservice.Shared.Response.Permission;

namespace SecurityMicroservice.Application.IServices
{
    public interface IPermissionService : IBaseService<PermissionRequestDto, PermissionResponseDto>
    {
    }
}
