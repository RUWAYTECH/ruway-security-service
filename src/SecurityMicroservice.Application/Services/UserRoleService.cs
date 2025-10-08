using AutoMapper;
using Ruway.Events.Command.Interfaces.Events;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.UserRole;
using SecurityMicroservice.Shared.Response.Common;
using System.Linq.Expressions;
using Ruway.Events.Command.Interfaces.Enums;

namespace SecurityMicroservice.Application.Services;


public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUserRepository _userRepository;

    public UserRoleService(
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        IMapper mapper,
        IEventPublisher eventPublisher,
        IUserRepository userRepository)
    {
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
        _userRepository = userRepository;
    }

    public async Task<List<UserRoleDto>> GetAllAsync()
    {
        var result = ResponseDto.Create<List<UserRoleDto>>();
        try
        {
            var userRoles = await _userRoleRepository.GetAllAsync();
            return _mapper.Map<List<UserRoleDto>>(userRoles);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<List<UserRoleDto>>(ex.Message);
        }
        return result.Data;
    }

    public async Task<ResponseDto<UserRoleDto>> GetByIdAsync(Guid userId, Guid roleId)
    {
        var result = ResponseDto.Create<UserRoleDto>();
        try
        {
            var userRole = await _userRoleRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId && x.RoleId == roleId, includeProperties: [y => y.User, r => r.Role]);
            result.Data = userRole != null ? _mapper.Map<UserRoleDto>(userRole) : null;
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserRoleDto>(ex.Message);
        }
        return result;
    }

    public async Task<List<UserRoleDto>> GetByUserIdAsync(Guid userId)
    {
        var result = ResponseDto.Create<List<UserRoleDto>>();
        try
        {
            var userRoles = await _userRoleRepository.GetByUserIdAsync(userId);
            result.Data = _mapper.Map<List<UserRoleDto>>(userRoles);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<List<UserRoleDto>>(ex.Message);
        }
        return result.Data;
    }

    public async Task<List<UserRoleDto>> GetByRoleIdAsync(Guid roleId)
    {
        var result = ResponseDto.Create<List<UserRoleDto>>();
        try
        {
            var userRoles = await _userRoleRepository.GetByRoleIdAsync(roleId);
            result.Data = _mapper.Map<List<UserRoleDto>>(userRoles);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<List<UserRoleDto>>(ex.Message);
        }
        return result.Data;
    }

    public async Task<ResponseDto<UserRoleDto>> CreateAsync(CreateUserRoleRequest request)
    {
        var result = ResponseDto.Create<UserRoleDto>();
        try
        {
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

            _userRoleRepository.Insert(userRole);
            result.Data = _mapper.Map<UserRoleDto>(userRole);

            await PublishEventsAsync(userRole.UserId, userRole.RoleId, UserActions.Created);
            
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserRoleDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<UserRoleDto>> UpdateAsync(Guid userId, Guid roleId, UpdateUserRoleRequest request)
    {
        var result = ResponseDto.Create<UserRoleDto>();
        try
        {
            var userRole = await _userRoleRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId && x.RoleId == roleId, includeProperties: [y => y.User, r => r.Role]);
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

            _userRoleRepository.Update(userRole);
            result.Data = _mapper.Map<UserRoleDto>(userRole);

            await PublishEventsAsync(userId, roleId, UserActions.Updated);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserRoleDto>(ex.Message);
        }
        return result;
    }

    private async Task PublishEventsAsync(Guid userId, Guid roleId, UserActions action)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId);
        var role = await _roleRepository.GetFirstOrDefaultAsync(filter: x => x.RoleId == roleId, includeProperties: [r => r.Application]);

        var userRoleAssignedEvent = new UserRoleAssignedEvent(
            user.UserId,
            role.Code ?? "",
            role.Name ?? "",
            ApplicationCode: role.Application.Code ?? "",
            Actions: action
            );

        await _eventPublisher.PublishAsync(userRoleAssignedEvent);
    }

    public async Task<ResponseDto> DeleteAsync(Guid userId, Guid roleId)
    {
        var result = ResponseDto.Create();
        try
        {
            var userRole = await _userRoleRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId && x.RoleId == roleId, includeProperties: [y => y.User, r => r.Role]);
            if (userRole == null)
            {
                result = ResponseDto.Error("No se pudo encontrar la asignación de rol para el usuario.");
                return result;
            }

            _userRoleRepository.Delete(userId, roleId);

            await PublishEventsAsync(userId, roleId, UserActions.Deleted);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error(ex.Message);
        }
        return result;
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
                pageSize: requestDto.PageSize,
                includeProperties: [x => x.Role, y => y.User]
            );

            response.Data = new PaginationResponseDto<UserRoleDto>
            {
                Items = _mapper.Map<IEnumerable<UserRoleDto>>(items),
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize,
            };
        }
        catch (Exception ex)
        {
            response = ResponseDto.Error<PaginationResponseDto<UserRoleDto>>(ex.Message);
        }
        return response;
    }
}