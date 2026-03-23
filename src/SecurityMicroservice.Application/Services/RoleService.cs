using AutoMapper;
using Microsoft.AspNetCore.Http;
using Rokys.Memo.Common.Constant;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Extensions;
using SecurityMicroservice.Shared.Response.Common;
using System.Linq.Expressions;

namespace SecurityMicroservice.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RoleService(
        IRoleRepository roleRepository,
        IApplicationRepository applicationRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _roleRepository = roleRepository;
        _applicationRepository = applicationRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        return _mapper.Map<List<RoleDto>>(roles);
    }

    public async Task<ResponseDto<RoleDto>> GetByIdAsync(Guid roleId)
    {
        var result = ResponseDto.Create<RoleDto>();
        try
        {
            var role = await _roleRepository.GetByKeyAsync(roleId);
            if (role != null)
            {
                result.Data = _mapper.Map<RoleDto>(role);
            }
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<RoleDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<RoleDto>> GetByCodeAsync(string code, Guid applicationId)
    {
        var result = ResponseDto.Create<RoleDto>();
        try
        {
            var role = await _roleRepository.GetFirstOrDefaultAsync(
                filter: x => x.Code == code && x.ApplicationId == applicationId && x.IsActive);
            if (role != null)
            {
                result.Data = _mapper.Map<RoleDto>(role);
            }
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<RoleDto>(ex.Message);
        }
        return result;
    }

    public async Task<List<RoleDto>> GetByApplicationIdAsync(Guid applicationId)
    {
        var allRoles = await _roleRepository.GetAsync(a=> a.Application.ApplicationId == applicationId && a.IsActive);
        return _mapper.Map<List<RoleDto>>(allRoles);
    }

    public async Task<ResponseDto<RoleDto>> CreateAsync(CreateRoleRequest request)
    {
        var result = ResponseDto.Create<RoleDto>();
        try
        {
            // Validar que la aplicación existe y está activa
            var application = await _applicationRepository.GetByKeyAsync(request.ApplicationId);
            if (application == null || !application.IsActive)
            {
                return ResponseDto.Error<RoleDto>("La aplicación especificada no existe o no está activa.");
            }

            // Validar que no existe un rol con el mismo código en la aplicación
            var existingRole = await _roleRepository.GetFirstOrDefaultAsync(
                filter: x => x.Code == request.Code && x.ApplicationId == request.ApplicationId);
            if (existingRole != null)
            {
                return ResponseDto.Error<RoleDto>("Ya existe un rol con el mismo código en esta aplicación.");
            }

            var role = new Domain.Entities.Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = request.ApplicationId,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _roleRepository.Insert(role);
            result.Data = _mapper.Map<RoleDto>(role);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<RoleDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<RoleDto>> UpdateAsync(Guid roleId, UpdateRoleRequest request)
    {
        var result = ResponseDto.Create<RoleDto>();
        try
        {
            var role = await _roleRepository.GetByKeyAsync(roleId);
            if (role == null)
            {
                return ResponseDto.Error<RoleDto>("Rol no encontrado.");
            }

            // Validar código único si se está cambiando
            if (!string.IsNullOrEmpty(request.Code) && request.Code != role.Code)
            {
                var existingRole = await _roleRepository.GetFirstOrDefaultAsync(
                    filter: x => x.Code == request.Code && x.ApplicationId == role.ApplicationId && x.RoleId != roleId);
                if (existingRole != null)
                {
                    return ResponseDto.Error<RoleDto>("Ya existe un rol con el mismo código en esta aplicación.");
                }
                role.Code = request.Code;
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                role.Name = request.Name;
            }

            if (request.Description != null)
            {
                role.Description = request.Description;
            }

            if (request.IsActive.HasValue)
            {
                role.IsActive = request.IsActive.Value;
            }

            role.UpdatedAt = DateTime.UtcNow;

            _roleRepository.Update(role);
            result.Data = _mapper.Map<RoleDto>(role);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<RoleDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto> DeleteAsync(Guid roleId)
    {
        var result = ResponseDto.Create();
        try
        {
            var role = await _roleRepository.GetByKeyAsync(roleId);
            if (role == null)
            {
                return ResponseDto.Error("Rol no encontrado.");
            }

            // Soft delete - marcar como inactivo
            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;

            _roleRepository.Update(role);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<PaginationResponseDto<RoleDto>>> GetPagedAsync(RoleFilterRequestDto requestDto)
    {
        var response = ResponseDto.Create<PaginationResponseDto<RoleDto>>();
        try
        {
            Expression<Func<Domain.Entities.Role, bool>>? filter = a => true;
            if (requestDto.ApplicationId != Guid.Empty)
            {
                filter = filter.AndAlso(r => r.ApplicationId == requestDto.ApplicationId);
            }
            if (!string.IsNullOrEmpty(requestDto.ApplicationCode))
            {
                filter = filter.AndAlso(r => r.Application.Code == requestDto.ApplicationCode);
            }

            var currentUser = _httpContextAccessor.CurrentUser();
            if (currentUser.IsAppAdmin && !currentUser.IsSuperAdmin)
            {
                var userApplicationCodes = currentUser.Roles?
                    .Where(a => a.Code == RoleCodes.ApplicationAdmin)
                    .Select(r => r.ApplicationCode)
                    .Distinct()
                    .ToList() ?? new List<string>();
                filter = filter.AndAlso(app => userApplicationCodes.Contains(app.Application.Code));
            }

            // Filtro de búsqueda por texto
            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                var searchFilter = requestDto.Filter.ToLower();
                filter = role => role.Name.ToLower().Contains(searchFilter) ||
                               role.Code.ToLower().Contains(searchFilter) ||
                               (role.Description != null && role.Description.ToLower().Contains(searchFilter)) ||
                               role.Application.Name.ToLower().Contains(searchFilter) ||
                               role.Application.Code.ToLower().Contains(searchFilter);
            }

            if (requestDto.ListInactive == null)
            {
                filter = filter.AndAlso(r => r.IsActive == true);
            }

            // Ordenamiento por defecto: por fecha de creación descendente
            Func<IQueryable<Domain.Entities.Role>, IOrderedQueryable<Domain.Entities.Role>> orderBy =
                q => q.OrderByDescending(x => x.CreatedAt);

            var (items, totalRows) = await _roleRepository.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                pageNumber: requestDto.PageNumber,
                pageSize: requestDto.PageSize,
                includeProperties: [x => x.Application]
            );

            response.Data = new PaginationResponseDto<RoleDto>
            {
                Items = _mapper.Map<IEnumerable<RoleDto>>(items),
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };
        }
        catch (Exception ex)
        {
            response = ResponseDto.Error<PaginationResponseDto<RoleDto>>(ex.Message);
        }
        return response;
    }
}