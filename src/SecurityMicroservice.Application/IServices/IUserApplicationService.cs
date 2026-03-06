using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.UserApplication;
using SecurityMicroservice.Shared.Response.Common;

namespace SecurityMicroservice.Application.IServices
{
    public interface IUserApplicationService
    {
        Task<List<UserApplicationDto>> GetAllAsync();
        Task<ResponseDto<UserApplicationDto>> GetByIdAsync(Guid userId, Guid applicationId);
        Task<List<UserApplicationDto>> GetByUserIdAsync(Guid userId);
        Task<List<UserApplicationDto>> GetByApplicationIdAsync(Guid applicationId);
        Task<ResponseDto<UserApplicationDto>> CreateAsync(CreateUserApplicationRequest request, bool isPublishEvent = true);
        Task<ResponseDto<UserApplicationDto>> UpdateAsync(Guid userId, Guid applicationId, UpdateUserApplicationRequest request);
        Task<ResponseDto> DeleteAsync(Guid userId, Guid applicationId);
        Task<ResponseDto<PaginationResponseDto<UserApplicationDto>>> GetPagedAsync(UserApplicationPaginationRequestDto requestDto);
    }
}
