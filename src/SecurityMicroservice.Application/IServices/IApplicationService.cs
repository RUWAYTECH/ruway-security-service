using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Response.Common;

namespace SecurityMicroservice.Application.IServices
{
    public interface IApplicationService
    {
        Task<List<ApplicationDto>> GetAllAsync();
        Task<ResponseDto<ApplicationDto>> GetByIdAsync(Guid applicationId);
        Task<ResponseDto<ApplicationDto>> GetByCodeAsync(string code);
        Task<ResponseDto<ApplicationDto>> CreateAsync(CreateApplicationRequest request);
        Task<ResponseDto<ApplicationDto>> UpdateAsync(Guid applicationId, UpdateApplicationRequest request);
        Task<ResponseDto> DeleteAsync(Guid applicationId);
        Task<ResponseDto<PaginationResponseDto<ApplicationDto>>> GetPagedAsync(PaginationRequestDto requestDto);
    }
}