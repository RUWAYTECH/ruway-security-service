using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Response.Common;

namespace SecurityMicroservice.Application.IServices
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllAsync();
        Task<ResponseDto<RoleDto>> GetByIdAsync(Guid roleId);
        Task<ResponseDto<RoleDto>> GetByCodeAsync(string code, Guid applicationId);
        Task<List<RoleDto>> GetByApplicationIdAsync(Guid applicationId);
        Task<ResponseDto<RoleDto>> CreateAsync(CreateRoleRequest request);
        Task<ResponseDto<RoleDto>> UpdateAsync(Guid roleId, UpdateRoleRequest request);
        Task<ResponseDto> DeleteAsync(Guid roleId);
        Task<ResponseDto<PaginationResponseDto<RoleDto>>> GetPagedAsync(PaginationRequestDto requestDto);
    }
}