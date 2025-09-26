using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.Request.Permission;
using SecurityMicroservice.Shared.Response.Common;
using SecurityMicroservice.Shared.Response.Permission;

namespace SecurityMicroservice.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;

        public async Task<ResponseDto<PermissionResponseDto>> Create(PermissionRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto<PaginationResponseDto<PermissionResponseDto>>> GetAll(PaginationRequestDto paginationRequestDto)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<PermissionResponseDto>> Update(object id, PermissionRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto> Delete(object id)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<PermissionResponseDto>> GetById(object id)
        {
            throw new NotImplementedException();
        }
    }
}
