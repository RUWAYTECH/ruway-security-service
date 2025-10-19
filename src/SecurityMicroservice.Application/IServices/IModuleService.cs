using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Response.Common;

namespace SecurityMicroservice.Application.IServices
{
    public interface IModuleService
    {
        Task<ResponseDto<ModuleManagementDto>> GetByIdAsync(Guid moduleId);
        Task<ResponseDto<ModuleManagementDto>> GetByCodeAsync(string code, Guid applicationId);
        Task<List<ModuleManagementDto>> GetByApplicationIdAsync(Guid applicationId);
        Task<ResponseDto<ModuleManagementDto>> CreateAsync(CreateModuleRequest request);
        Task<ResponseDto<ModuleManagementDto>> UpdateAsync(Guid moduleId, UpdateModuleRequest request);
        Task<ResponseDto> DeleteAsync(Guid moduleId);
        Task<ResponseDto<PaginationResponseDto<ModuleManagementDto>>> GetPagedAsync(ModuleFilterRequestDto requestDto);
    }
}