using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.UserRole;
using SecurityMicroservice.Shared.Response.Common;

namespace SecurityMicroservice.Application.IServices
{
    public interface IUserRoleService
    {
        Task<List<UserRoleDto>> GetAllAsync();
        Task<ResponseDto<UserRoleDto>> GetByIdAsync(Guid userId, Guid roleId);
        Task<List<UserRoleDto>> GetByUserIdAsync(Guid userId);
        Task<List<UserRoleDto>> GetByRoleIdAsync(Guid roleId);
        Task<ResponseDto<UserRoleDto>> CreateAsync(CreateUserRoleRequest request);
        Task<ResponseDto<UserRoleDto>> UpdateAsync(Guid userId, Guid roleId, UpdateUserRoleRequest request);
        Task<ResponseDto> DeleteAsync(Guid userId, Guid roleId);
        Task<ResponseDto<PaginationResponseDto<UserRoleDto>>> GetPagedAsync(UserRolePaginationRequestDto requestDto);
    }
}
