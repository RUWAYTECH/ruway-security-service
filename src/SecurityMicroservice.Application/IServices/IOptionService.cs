using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.Option;
using SecurityMicroservice.Shared.Response.Common;

namespace SecurityMicroservice.Application.IServices;

public interface IOptionService
{
    Task<List<OptionDto>> GetAllAsync();
    Task<OptionDto?> GetByIdAsync(Guid optionId);
    Task<List<OptionDto>> GetByModuleIdAsync(Guid moduleId);
    Task<List<OptionDto>> GetByApplicationCodeAsync(string applicationCode);
    Task<OptionDto?> GetByCodeAsync(string code);
    Task<ResponseDto<OptionDto>> CreateAsync(CreateOptionRequest request);
    Task<ResponseDto<OptionDto>> UpdateAsync(Guid optionId, UpdateOptionRequest request);
    Task<ResponseDto> DeleteAsync(Guid optionId);
    Task<ResponseDto<PaginationResponseDto<OptionDto>>> GetPagedAsync(OptionPaginationRequestDto requestDto);
}