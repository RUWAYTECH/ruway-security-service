using AutoMapper;
using Ruway.Events.Command.Interfaces.Events;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Extensions;
using SecurityMicroservice.Shared.Request.UserApplication;
using SecurityMicroservice.Shared.Response.Common;
using System.Linq.Expressions;

namespace SecurityMicroservice.Application.Services;

public class UserApplicationService : IUserApplicationService
{
    private readonly IUserApplicationRepository _userApplicationRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleService _userRoleService;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;

    private readonly IApplicationRepository _applicationRepository;


    public UserApplicationService(
        IUserApplicationRepository userApplicationRepository,
        IMapper mapper,
        IEventPublisher eventPublisher,
        IUserRepository userRepository,
        IApplicationRepository applicationRepository,
        IUserRoleService userRoleService,
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository
        )
    {
        _userApplicationRepository = userApplicationRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _userRoleService = userRoleService;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
    }

    public async Task<List<UserApplicationDto>> GetAllAsync()
    {
        var userApplications = await _userApplicationRepository.GetAllAsync();
        return _mapper.Map<List<UserApplicationDto>>(userApplications);
    }

    public async Task<ResponseDto<UserApplicationDto>> GetByIdAsync(Guid userId, Guid applicationId)
    {
        var result = ResponseDto.Create<UserApplicationDto>();
        try
        {
            var userApplication = await _userApplicationRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId && x.ApplicationId == applicationId && x.IsActive);
            result.Data = userApplication != null ? _mapper.Map<UserApplicationDto>(userApplication) : null;
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserApplicationDto>(ex.Message);
        }
        return result;
    }

    public async Task<List<UserApplicationDto>> GetByUserIdAsync(Guid userId)
    {
        var result = ResponseDto.Create<List<UserApplicationDto>>();
        try
        {
            var userApplications = await _userApplicationRepository.GetByUserIdAsync(userId);
            result.Data = _mapper.Map<List<UserApplicationDto>>(userApplications);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<List<UserApplicationDto>>(ex.Message);
        }
        return result.Data;
    }

    public async Task<List<UserApplicationDto>> GetByApplicationIdAsync(Guid applicationId)
    {
        var result = ResponseDto.Create<List<UserApplicationDto>>();
        try
        {
            var userApplications = await _userApplicationRepository.GetByApplicationIdAsync(applicationId);
            result.Data = _mapper.Map<List<UserApplicationDto>>(userApplications);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<List<UserApplicationDto>>(ex.Message);
        }
        return result.Data;
    }

    public async Task<ResponseDto<UserApplicationDto>> CreateAsync(CreateUserApplicationRequest request, bool isPublishEvent = true)
    {
        var result = ResponseDto.Create<UserApplicationDto>();
        try
        {
            var exists = await _userApplicationRepository.GetFirstOrDefaultAsync(a => a.UserId == request.UserId && a.ApplicationId == request.ApplicationId);
            if (exists != null)
            {
                if (exists.IsActive)
                {
                    throw new InvalidOperationException("El usuario ya tiene asignada esta aplicación.");
                }
                else
                {
                    exists.IsActive = true;
                    exists.RevokedAt = null;
                    exists.UpdatedAt = DateTime.UtcNow;

                    _userApplicationRepository.Update(exists);
                    result.Data = _mapper.Map<UserApplicationDto>(exists);
                    if (request.RoleIds != null && request.RoleIds.Count > 0)
                    {
                        await _userRoleService.DeleteByUserAndApplicationAsync(exists.UserId, exists.ApplicationId);
                        foreach (var roleId in request.RoleIds)
                        {
                            var userRoles = await _userRoleService.CreateAsync(new Shared.Request.UserRole.CreateUserRoleRequest
                            {
                                UserId = request.UserId,
                                RoleId = roleId
                            });
                            result.Messages.AddRange(userRoles.Messages);
                        }
                    }
                    if (isPublishEvent)
                    {
                        await PublishEventsAsync(exists.UserId, exists.ApplicationId, request.RoleIds);
                    }
                    return result;
                }
            }

            var userApplication = new UserApplication
            {
                UserId = request.UserId,
                ApplicationId = request.ApplicationId,
                Notes = request.Notes,
                AssignedAt = DateTime.UtcNow
            };

            _userApplicationRepository.Insert(userApplication);
            result.Data = _mapper.Map<UserApplicationDto>(userApplication);


            var user = await _userRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == request.UserId && x.Status == UserStatus.Active);
            var application = await _applicationRepository.GetFirstOrDefaultAsync(filter: x => x.ApplicationId == request.ApplicationId && x.IsActive);

            if (application == null)
            {
                throw new InvalidOperationException("La aplicación no está activa.");
            }


            if (request.RoleIds != null && request.RoleIds.Count > 0)
            {
                if (exists != null)
                {
                    await _userRoleService.DeleteByUserAndApplicationAsync(exists.UserId, exists.ApplicationId);
                }

                foreach (var roleId in request.RoleIds)
                {
                    var userRoles = await _userRoleService.CreateAsync(new Shared.Request.UserRole.CreateUserRoleRequest
                    {
                        UserId = request.UserId,
                        RoleId = roleId
                    });
                    result.Messages.AddRange(userRoles.Messages);
                }
            }
            if (isPublishEvent)
            {
                await PublishEventsAsync(user.UserId, application.ApplicationId);
            }

        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserApplicationDto>(ex.Message);
        }
        return result;
    }


    public async Task<ResponseDto<UserApplicationDto>> UpdateAsync(Guid userId, Guid applicationId, UpdateUserApplicationRequest request)
    {
        var result = ResponseDto.Create<UserApplicationDto>();
        try
        {
            var userApplication = await _userApplicationRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId && x.ApplicationId == applicationId);
            if (userApplication == null) return null;

            if (request.IsActive.HasValue)
            {
                userApplication.IsActive = request.IsActive.Value;
                if (!request.IsActive.Value)
                {
                    userApplication.RevokedAt = DateTime.UtcNow;
                }
                else
                {
                    userApplication.RevokedAt = null;
                }
            }

            if (!string.IsNullOrEmpty(request.Notes))
            {
                userApplication.Notes = request.Notes;
            }

            userApplication.UpdatedAt = DateTime.UtcNow;

            _userApplicationRepository.Update(userApplication);
            result.Data = _mapper.Map<UserApplicationDto>(userApplication);

            if (request.RoleIds != null && request.RoleIds.Count > 0)
            {
                await _userRoleService.DeleteByUserAndApplicationAsync(userApplication.UserId, userApplication.ApplicationId);
                foreach (var roleId in request.RoleIds)
                {
                    var userRoles = await _userRoleService.CreateAsync(new Shared.Request.UserRole.CreateUserRoleRequest
                    {
                        UserId = userApplication.UserId,
                        RoleId = roleId
                    });
                    result.Messages.AddRange(userRoles.Messages);
                }
            }

            await PublishEventsAsync(userId, applicationId, request.RoleIds);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserApplicationDto>(ex.Message);
        }
        return result;
    }

    private async Task PublishEventsAsync(Guid userId, Guid applicationId, List<Guid>? roleIds = null)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId);
        var application = await _applicationRepository.GetFirstOrDefaultAsync(filter: x => x.ApplicationId == applicationId && x.IsActive);
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId, applicationId);
        var rolesUpdate = await _roleRepository.GetAsync(filter: x => roleIds != null && roleIds.Contains(x.RoleId));
        var userUpdatedEvent = new UserUpdatedEvent(
            UserId: user.UserId,
            EmployeeId: user.EmployeeId,
            UserName: user.UserName,
            FirstName: user.FirstName ?? "",
            LastName: user.LastName ?? "",
            Email: user.Email ?? "",
            ApplicationCode: application.Code ?? "",
            RoleCodes: string.Join(",", rolesUpdate.Select(a => a.Code ?? "")),
            RoleNames: string.Join(",", rolesUpdate.Select(a => a.Name ?? ""))
            );

        await _eventPublisher.PublishAsync(userUpdatedEvent);
    }
    public async Task<ResponseDto> DeleteAsync(Guid userId, Guid applicationId, bool isPublishEvent = true)
    {
        var result = ResponseDto.Create();
        try
        {
            var userApplication = await _userApplicationRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId && x.ApplicationId == applicationId);
            if (userApplication == null)
            {
                result = ResponseDto.Error("No se pudo encontrar la asignación de aplicación para el usuario.");
                return result;
            }
            var application = await _applicationRepository.GetFirstOrDefaultAsync(filter: x => x.ApplicationId == applicationId && x.IsActive);
            if (application == null)
            {
                result = ResponseDto.Error("La aplicación no existe o no está activa.");
                return result;
            }

            userApplication.UpdatedAt = DateTime.UtcNow;
            userApplication.IsActive = false;
            userApplication.RevokedAt = DateTime.UtcNow;
            _userApplicationRepository.Update(userApplication);
            var userUpdatedEvent = new UserDeletedEvent(
                UserId: userId,
                ApplicationCode: application.Code ?? ""
                );

            await _userRoleService.DeleteByUserAndApplicationAsync(userId, application.ApplicationId);
            if (isPublishEvent)
            {
                await _eventPublisher.PublishAsync(userUpdatedEvent);
            }
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<PaginationResponseDto<UserApplicationDto>>> GetPagedAsync(UserApplicationPaginationRequestDto requestDto)
    {
        var response = ResponseDto.Create<PaginationResponseDto<UserApplicationDto>>();
        try
        {
            Expression<Func<UserApplication, bool>>? filter = a => a.IsActive;

            if (requestDto.UserId.HasValue)
            {
                filter = ua => ua.UserId == requestDto.UserId.Value && ua.IsActive;
            }

            if (requestDto.ApplicationId.HasValue)
            {
                filter = filter.AndAlso(x => x.ApplicationId == requestDto.ApplicationId.Value);
            }

            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                filter = filter.AndAlso(x => x.Application.Name.ToLower().Contains(requestDto.Filter.ToLower()) ||
                 x.Application.Code.ToLower().Contains(requestDto.Filter.ToLower()) ||
                 (x.User.FirstName + " " + x.User.LastName).ToLower().Contains(requestDto.Filter.ToLower()) ||
                        x.User.FirstName.ToLower().Contains(requestDto.Filter.ToLower()) ||
                        x.User.LastName.ToLower().Contains(requestDto.Filter.ToLower()) ||
                        x.User.UserName.ToLower().Contains(requestDto.Filter.ToLower()));
            }

            Func<IQueryable<UserApplication>, IOrderedQueryable<UserApplication>> orderBy = q => q.OrderByDescending(x => x.AssignedAt);

            var (items, totalRows) = await _userApplicationRepository.GetUserApplicationPagedAsync(
                filter: filter,
                orderBy: orderBy,
                pageNumber: requestDto.PageNumber,
                pageSize: requestDto.PageSize
            );

            response.Data = new PaginationResponseDto<UserApplicationDto>
            {
                Items = _mapper.Map<IEnumerable<UserApplicationDto>>(items),
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };
        }
        catch (Exception ex)
        {
            response = ResponseDto.Error<PaginationResponseDto<UserApplicationDto>>(ex.Message);
        }
        return response;
    }

    private Expression<Func<UserApplication, bool>> CombineFilters(Expression<Func<UserApplication, bool>> filter, Func<object, bool> value)
    {
        throw new NotImplementedException();
    }
}