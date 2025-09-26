using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.Permission;
using SecurityMicroservice.Shared.Response.Common;
using SecurityMicroservice.Shared.Response.Permission;
using System.Threading;

namespace SecurityMicroservice.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IMapper _mapper;
        public PermissionService(IPermissionRepository permissionRepository, IMapper mapper)
        {
            _permissionRepository = permissionRepository;
            _mapper = mapper;
        }

        public async Task<ResponseDto<PermissionResponseDto>> Create(PermissionRequestDto requestDto)
        {
            var result = ResponseDto.Create<PermissionResponseDto>();
            try
            {
                var entity = _mapper.Map<Permission>(requestDto);
                _permissionRepository.Insert(entity);
                result.Data = _mapper.Map<PermissionResponseDto>(entity);
            }
            catch (Exception ex)
            {
                result = ResponseDto.Error<PermissionResponseDto>(ex.Message);
            }
            return result;
        }

        public async Task<ResponseDto<PaginationResponseDto<PermissionResponseDto>>> GetPaged(PaginationRequestDto paginationRequestDto)
        {
            var result = ResponseDto.Create<PaginationResponseDto<PermissionResponseDto>>();
            try
            {
                System.Linq.Expressions.Expression<System.Func<Permission, bool>> filter = x => x.IsActive;

                Func<IQueryable<Permission>, IOrderedQueryable<Permission>> orderBy = q => q.OrderBy(x => x.CreatedAt);

                var (items, totalRows) = await _permissionRepository.GetPagedAsync(
                    filter: filter,
                    orderBy: orderBy,
                    pageNumber: paginationRequestDto.PageNumber,
                    pageSize: paginationRequestDto.PageSize
                );

                result.Data = new PaginationResponseDto<PermissionResponseDto>
                {
                    Items = _mapper.Map<IEnumerable<PermissionResponseDto>>(items),
                    TotalCount = totalRows,
                    PageNumber = paginationRequestDto.PageNumber,
                    PageSize = paginationRequestDto.PageSize
                };
            }
            catch (Exception ex)
            {
                result = ResponseDto.Error<PaginationResponseDto<PermissionResponseDto>>(ex.Message);
            }
            return result;
        }

        public async Task<ResponseDto<PermissionResponseDto>> Update(object id, PermissionRequestDto dto)
        {
            var result = ResponseDto.Create<PermissionResponseDto>();
            try
            {
                var permission = await _permissionRepository.GetByKeyAsync(id);
                if (permission == null)
                {
                    result = ResponseDto.Error<PermissionResponseDto>("No se pudo encontrar el permiso");
                    return result;
                }

                _mapper.Map(dto, permission);

                _permissionRepository.Update(permission);

                result.Data = _mapper.Map<PermissionResponseDto>(permission);
            }
            catch (Exception ex)
            {
                result = ResponseDto.Error<PermissionResponseDto>(ex.Message);
            }
            return result;
        }

        public async Task<ResponseDto> Delete(object id)
        {
            var result = ResponseDto.Create();
            try
            {
                var entity = await _permissionRepository.GetFirstOrDefaultAsync(filter: x => x.PermissionId == (Guid)id && x.IsActive);
                if (entity == null)
                {
                    result = ResponseDto.Error("No se pudo encontrar el permiso");
                    return result;
                }
                _permissionRepository.Delete(entity);
            } catch (Exception ex)
            {
                result = ResponseDto.Error(ex.Message);
            }
            return result;
        }

        public async Task<ResponseDto<PermissionResponseDto>> GetById(object id)
        {
            var result = ResponseDto.Create<PermissionResponseDto>();
            try
            {
                var entity = await _permissionRepository.GetFirstOrDefaultAsync(filter: x => x.PermissionId == (Guid)id && x.IsActive);
                if (entity == null)
                {
                    result = ResponseDto.Error<PermissionResponseDto>("No se pudo encontrar el permiso");
                    return result;
                }
                result.Data = _mapper.Map<PermissionResponseDto>(entity);

            } catch (Exception ex)
            {
                result = ResponseDto.Error<PermissionResponseDto>(ex.Message);
            }
            return result;
        }
    }
}
