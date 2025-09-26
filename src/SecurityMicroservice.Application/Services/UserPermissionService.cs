using AutoMapper;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.UserPermission;
using SecurityMicroservice.Shared.Response.Common;
using System.Linq.Expressions;
using System.Security.Claims;

namespace SecurityMicroservice.Application.Services;

public interface IUserPermissionService
{
    Task<List<UserPermissionDto>> GetAllAsync();
    Task<UserPermissionDto?> GetByIdAsync(Guid userId, Guid permissionId);
    Task<List<UserPermissionDto>> GetByUserIdAsync(Guid userId);
    Task<List<UserPermissionDto>> GetByPermissionIdAsync(Guid permissionId);
    Task<UserPermissionDto> CreateAsync(CreateUserPermissionRequest request, Guid? grantedByUserId = null);
    Task<UserPermissionDto?> UpdateAsync(Guid userId, Guid permissionId, UpdateUserPermissionRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid permissionId);
    Task<ResponseDto<PaginationResponseDto<UserPermissionDto>>> GetPagedAsync(UserPermissionPaginationRequestDto requestDto);
}

public class UserPermissionService : IUserPermissionService
{
    private readonly IUserPermissionRepository _userPermissionRepository;
    private readonly IMapper _mapper;

    public UserPermissionService(
        IUserPermissionRepository userPermissionRepository,
        IMapper mapper)
    {
        _userPermissionRepository = userPermissionRepository;
        _mapper = mapper;
    }

    public async Task<List<UserPermissionDto>> GetAllAsync()
    {
        var userPermissions = await _userPermissionRepository.GetAllAsync();
        return _mapper.Map<List<UserPermissionDto>>(userPermissions);
    }

    public async Task<UserPermissionDto?> GetByIdAsync(Guid userId, Guid permissionId)
    {
        var userPermission = await _userPermissionRepository.GetByIdAsync(userId, permissionId);
        return userPermission != null ? _mapper.Map<UserPermissionDto>(userPermission) : null;
    }

    public async Task<List<UserPermissionDto>> GetByUserIdAsync(Guid userId)
    {
        var userPermissions = await _userPermissionRepository.GetByUserIdAsync(userId);
        return _mapper.Map<List<UserPermissionDto>>(userPermissions);
    }

    public async Task<List<UserPermissionDto>> GetByPermissionIdAsync(Guid permissionId)
    {
        var userPermissions = await _userPermissionRepository.GetByPermissionIdAsync(permissionId);
        return _mapper.Map<List<UserPermissionDto>>(userPermissions);
    }

    public async Task<UserPermissionDto> CreateAsync(CreateUserPermissionRequest request, Guid? grantedByUserId = null)
    {
        // Verificar si ya existe el permiso
        var exists = await _userPermissionRepository.ExistsAsync(request.UserId, request.PermissionId);
        if (exists)
        {
            throw new InvalidOperationException("El usuario ya tiene asignado este permiso.");
        }

        var userPermission = new UserPermission
        {
            UserId = request.UserId,
            PermissionId = request.PermissionId,
            ExpiresAt = request.ExpiresAt,
            Reason = request.Reason,
            GrantedByUserId = grantedByUserId,
            IsActive = true,
            GrantedAt = DateTime.UtcNow
        };

        var created = await _userPermissionRepository.CreateAsync(userPermission);
        return _mapper.Map<UserPermissionDto>(created);
    }

    public async Task<UserPermissionDto?> UpdateAsync(Guid userId, Guid permissionId, UpdateUserPermissionRequest request)
    {
        var userPermission = await _userPermissionRepository.GetByIdAsync(userId, permissionId);
        if (userPermission == null) return null;

        if (request.ExpiresAt.HasValue)
        {
            userPermission.ExpiresAt = request.ExpiresAt;
        }

        if (!string.IsNullOrEmpty(request.Reason))
        {
            userPermission.Reason = request.Reason;
        }

        if (request.IsActive.HasValue)
        {
            userPermission.IsActive = request.IsActive.Value;
        }

        userPermission.UpdatedAt = DateTime.UtcNow;

        var updated = await _userPermissionRepository.UpdateAsync(userPermission);
        return _mapper.Map<UserPermissionDto>(updated);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid permissionId)
    {
        var userPermission = await _userPermissionRepository.GetByIdAsync(userId, permissionId);
        if (userPermission == null) return false;

        await _userPermissionRepository.DeleteAsync(userId, permissionId);
        return true;
    }

    public async Task<ResponseDto<PaginationResponseDto<UserPermissionDto>>> GetPagedAsync(UserPermissionPaginationRequestDto requestDto)
    {
        var response = ResponseDto.Create<PaginationResponseDto<UserPermissionDto>>();
        try
        {
            Expression<Func<UserPermission, bool>>? filter = null;

            if (requestDto.UserId.HasValue)
            {
                filter = up => up.UserId == requestDto.UserId.Value;
            }

            if (requestDto.PermissionId.HasValue)
            {
                var permissionFilter = new Func<Expression<Func<UserPermission, bool>>, Expression<Func<UserPermission, bool>>>(
                    existing => existing == null 
                        ? up => up.PermissionId == requestDto.PermissionId.Value
                        : up => existing.Compile()(up) && up.PermissionId == requestDto.PermissionId.Value);
                filter = permissionFilter(filter);
            }

            if (requestDto.ApplicationId.HasValue)
            {
                var appFilter = new Func<Expression<Func<UserPermission, bool>>, Expression<Func<UserPermission, bool>>>(
                    existing => existing == null 
                        ? up => up.Permission.Option.Module.ApplicationId == requestDto.ApplicationId.Value
                        : up => existing.Compile()(up) && up.Permission.Option.Module.ApplicationId == requestDto.ApplicationId.Value);
                filter = appFilter(filter);
            }

            if (!string.IsNullOrEmpty(requestDto.ApplicationCode))
            {
                var appCodeFilter = new Func<Expression<Func<UserPermission, bool>>, Expression<Func<UserPermission, bool>>>(
                    existing => existing == null 
                        ? up => up.Permission.Option.Module.Application.Code == requestDto.ApplicationCode
                        : up => existing.Compile()(up) && up.Permission.Option.Module.Application.Code == requestDto.ApplicationCode);
                filter = appCodeFilter(filter);
            }

            if (requestDto.IsActive.HasValue)
            {
                var activeFilter = new Func<Expression<Func<UserPermission, bool>>, Expression<Func<UserPermission, bool>>>(
                    existing => existing == null 
                        ? up => up.IsActive == requestDto.IsActive.Value
                        : up => existing.Compile()(up) && up.IsActive == requestDto.IsActive.Value);
                filter = activeFilter(filter);
            }

            if (!requestDto.IncludeExpired.GetValueOrDefault(true))
            {
                var notExpiredFilter = new Func<Expression<Func<UserPermission, bool>>, Expression<Func<UserPermission, bool>>>(
                    existing => existing == null 
                        ? up => up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow
                        : up => existing.Compile()(up) && (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow));
                filter = notExpiredFilter(filter);
            }

            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                var searchFilter = requestDto.Filter.ToLower();
                var textFilter = new Func<Expression<Func<UserPermission, bool>>, Expression<Func<UserPermission, bool>>>(
                    existing => existing == null 
                        ? up => up.User.Username.ToLower().Contains(searchFilter) ||
                                up.Permission.Option.Name.ToLower().Contains(searchFilter) ||
                                up.Permission.Option.Module.Name.ToLower().Contains(searchFilter) ||
                                up.Permission.Option.Module.Application.Name.ToLower().Contains(searchFilter)
                        : up => existing.Compile()(up) && 
                                (up.User.Username.ToLower().Contains(searchFilter) ||
                                 up.Permission.Option.Name.ToLower().Contains(searchFilter) ||
                                 up.Permission.Option.Module.Name.ToLower().Contains(searchFilter) ||
                                 up.Permission.Option.Module.Application.Name.ToLower().Contains(searchFilter)));
                filter = textFilter(filter);
            }

            Func<IQueryable<UserPermission>, IOrderedQueryable<UserPermission>> orderBy = q => q.OrderByDescending(x => x.GrantedAt);

            var (items, totalRows) = await _userPermissionRepository.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                pageNumber: requestDto.PageNumber,
                pageSize: requestDto.PageSize
            );

            response.Data = new PaginationResponseDto<UserPermissionDto>
            {
                Items = _mapper.Map<IEnumerable<UserPermissionDto>>(items),
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };
        }
        catch (Exception ex)
        {
            response = ResponseDto.Error<PaginationResponseDto<UserPermissionDto>>(ex.Message);
        }
        return response;
    }
}