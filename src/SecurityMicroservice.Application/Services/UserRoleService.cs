using AutoMapper;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.UserRole;
using SecurityMicroservice.Shared.Response.Common;
using System.Linq.Expressions;

namespace SecurityMicroservice.Application.Services;

public interface IUserRoleService
{
    Task<List<UserRoleDto>> GetAllAsync();
    Task<UserRoleDto?> GetByIdAsync(Guid userId, Guid roleId);
    Task<List<UserRoleDto>> GetByUserIdAsync(Guid userId);
    Task<List<UserRoleDto>> GetByRoleIdAsync(Guid roleId);
    Task<UserRoleDto> CreateAsync(CreateUserRoleRequest request);
    Task<UserRoleDto?> UpdateAsync(Guid userId, Guid roleId, UpdateUserRoleRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid roleId);
    Task<ResponseDto<PaginationResponseDto<UserRoleDto>>> GetPagedAsync(UserRolePaginationRequestDto requestDto);
}

public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IMapper _mapper;

    public UserRoleService(
        IUserRoleRepository userRoleRepository,
        IMapper mapper)
    {
        _userRoleRepository = userRoleRepository;
        _mapper = mapper;
    }

    public async Task<List<UserRoleDto>> GetAllAsync()
    {
        var userRoles = await _userRoleRepository.GetAllAsync();
        return _mapper.Map<List<UserRoleDto>>(userRoles);
    }

    public async Task<UserRoleDto?> GetByIdAsync(Guid userId, Guid roleId)
    {
        var userRole = await _userRoleRepository.GetByIdAsync(userId, roleId);
        return userRole != null ? _mapper.Map<UserRoleDto>(userRole) : null;
    }

    public async Task<List<UserRoleDto>> GetByUserIdAsync(Guid userId)
    {
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId);
        return _mapper.Map<List<UserRoleDto>>(userRoles);
    }

    public async Task<List<UserRoleDto>> GetByRoleIdAsync(Guid roleId)
    {
        var userRoles = await _userRoleRepository.GetByRoleIdAsync(roleId);
        return _mapper.Map<List<UserRoleDto>>(userRoles);
    }

    public async Task<UserRoleDto> CreateAsync(CreateUserRoleRequest request)
    {
        // Verificar si ya existe la asignación
        var exists = await _userRoleRepository.ExistsAsync(request.UserId, request.RoleId);
        if (exists)
        {
            throw new InvalidOperationException("El usuario ya tiene asignado este rol.");
        }

        var userRole = new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            Notes = request.Notes,
            AssignedAt = DateTime.UtcNow
        };

        var created = await _userRoleRepository.CreateAsync(userRole);
        return _mapper.Map<UserRoleDto>(created);
    }

    public async Task<UserRoleDto?> UpdateAsync(Guid userId, Guid roleId, UpdateUserRoleRequest request)
    {
        var userRole = await _userRoleRepository.GetByIdAsync(userId, roleId);
        if (userRole == null) return null;

        if (!string.IsNullOrEmpty(request.Notes))
        {
            userRole.Notes = request.Notes;
        }

        if (request.RevokedAt.HasValue)
        {
            userRole.RevokedAt = request.RevokedAt;
        }

        userRole.UpdatedAt = DateTime.UtcNow;

        var updated = await _userRoleRepository.UpdateAsync(userRole);
        return _mapper.Map<UserRoleDto>(updated);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid roleId)
    {
        var userRole = await _userRoleRepository.GetByIdAsync(userId, roleId);
        if (userRole == null) return false;

        await _userRoleRepository.DeleteAsync(userId, roleId);
        return true;
    }

    public async Task<ResponseDto<PaginationResponseDto<UserRoleDto>>> GetPagedAsync(UserRolePaginationRequestDto requestDto)
    {
        var response = ResponseDto.Create<PaginationResponseDto<UserRoleDto>>();
        try
        {
            Expression<Func<UserRole, bool>>? filter = null;

            if (requestDto.UserId.HasValue)
            {
                filter = ur => ur.UserId == requestDto.UserId.Value;
            }

            if (requestDto.RoleId.HasValue)
            {
                var roleFilter = new Func<Expression<Func<UserRole, bool>>, Expression<Func<UserRole, bool>>>(
                    existing => existing == null 
                        ? ur => ur.RoleId == requestDto.RoleId.Value
                        : ur => existing.Compile()(ur) && ur.RoleId == requestDto.RoleId.Value);
                filter = roleFilter(filter);
            }

            if (requestDto.ApplicationId.HasValue)
            {
                var appFilter = new Func<Expression<Func<UserRole, bool>>, Expression<Func<UserRole, bool>>>(
                    existing => existing == null 
                        ? ur => ur.Role.ApplicationId == requestDto.ApplicationId.Value
                        : ur => existing.Compile()(ur) && ur.Role.ApplicationId == requestDto.ApplicationId.Value);
                filter = appFilter(filter);
            }

            if (!string.IsNullOrEmpty(requestDto.ApplicationCode))
            {
                var appCodeFilter = new Func<Expression<Func<UserRole, bool>>, Expression<Func<UserRole, bool>>>(
                    existing => existing == null 
                        ? ur => ur.Role.Application.Code == requestDto.ApplicationCode
                        : ur => existing.Compile()(ur) && ur.Role.Application.Code == requestDto.ApplicationCode);
                filter = appCodeFilter(filter);
            }

            if (requestDto.IsActive.HasValue)
            {
                var activeFilter = new Func<Expression<Func<UserRole, bool>>, Expression<Func<UserRole, bool>>>(
                    existing => existing == null 
                        ? ur => requestDto.IsActive.Value ? ur.RevokedAt == null : ur.RevokedAt != null
                        : ur => existing.Compile()(ur) && (requestDto.IsActive.Value ? ur.RevokedAt == null : ur.RevokedAt != null));
                filter = activeFilter(filter);
            }

            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                var searchFilter = requestDto.Filter.ToLower();
                var textFilter = new Func<Expression<Func<UserRole, bool>>, Expression<Func<UserRole, bool>>>(
                    existing => existing == null 
                        ? ur => ur.User.Username.ToLower().Contains(searchFilter) ||
                                ur.Role.Name.ToLower().Contains(searchFilter) ||
                                ur.Role.Code.ToLower().Contains(searchFilter) ||
                                ur.Role.Application.Name.ToLower().Contains(searchFilter)
                        : ur => existing.Compile()(ur) && 
                                (ur.User.Username.ToLower().Contains(searchFilter) ||
                                 ur.Role.Name.ToLower().Contains(searchFilter) ||
                                 ur.Role.Code.ToLower().Contains(searchFilter) ||
                                 ur.Role.Application.Name.ToLower().Contains(searchFilter)));
                filter = textFilter(filter);
            }

            Func<IQueryable<UserRole>, IOrderedQueryable<UserRole>> orderBy = q => q.OrderByDescending(x => x.AssignedAt);

            var (items, totalRows) = await _userRoleRepository.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                pageNumber: requestDto.PageNumber,
                pageSize: requestDto.PageSize
            );

            response.Data = new PaginationResponseDto<UserRoleDto>
            {
                Items = _mapper.Map<IEnumerable<UserRoleDto>>(items),
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };
        }
        catch (Exception ex)
        {
            response = ResponseDto.Error<PaginationResponseDto<UserRoleDto>>(ex.Message);
        }
        return response;
    }
}