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

    private readonly IApplicationRepository _applicationRepository;


    public UserApplicationService(
        IUserApplicationRepository userApplicationRepository,
        IMapper mapper,
        IEventPublisher eventPublisher,
        IUserRepository userRepository,
        IApplicationRepository applicationRepository,
        IUserRoleService userRoleService,
        IUserRoleRepository userRoleRepository
        )
    {
        _userApplicationRepository = userApplicationRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _userRoleService = userRoleService;
        _userRoleRepository = userRoleRepository;
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

    public async Task<ResponseDto<UserApplicationDto>> CreateAsync(CreateUserApplicationRequest request)
    {
        var result = ResponseDto.Create<UserApplicationDto>();
        try
        {
            var exists = await _userApplicationRepository.ExistsAsync(request.UserId, request.ApplicationId);
            if (exists)
            {
                throw new InvalidOperationException("El usuario ya tiene asignada esta aplicación.");
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

            await PublishEventsAsync(user.UserId, application.ApplicationId);
            if (request.RoleId != Guid.Empty)
            {
                var userRoles = await _userRoleService.CreateAsync(new Shared.Request.UserRole.CreateUserRoleRequest
                {
                    UserId = request.UserId,
                    RoleId = request.RoleId
                });
                result.Messages.AddRange(userRoles.Messages);
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

            await PublishEventsAsync(userId, applicationId);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<UserApplicationDto>(ex.Message);
        }
        return result;
    }

    private async Task PublishEventsAsync(Guid userId, Guid applicationId)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(filter: x => x.UserId == userId);
        var application = await _applicationRepository.GetFirstOrDefaultAsync(filter: x => x.ApplicationId == applicationId && x.IsActive);
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId);
        var userUpdatedEvent = new UserUpdatedEvent(
            user.UserId,
            user.EmployeeId,
            user.UserName,
            user.FirstName ?? "",
            user.LastName ?? "",
            user.Email ?? "",
            ApplicationCode: application.Code ?? "",
            RoleCodes: userRoles.Select(a => a.Role.Code ?? "").ToList().ToString(),
            RoleNames: userRoles.Select(a => a.Role.Name ?? "").ToList().ToString()
            );

        await _eventPublisher.PublishAsync(userUpdatedEvent);
    }
    public async Task<ResponseDto> DeleteAsync(Guid userId, Guid applicationId)
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

            userApplication.UpdatedAt = DateTime.UtcNow;
            userApplication.IsActive = false;
            userApplication.RevokedAt = DateTime.UtcNow;
            _userApplicationRepository.Update(userApplication);
            var userUpdatedEvent = new UserDeletedEvent(
                UserId: userId,
                ApplicationCode: application.Code ?? ""
                );

            await _eventPublisher.PublishAsync(userUpdatedEvent);

            await _userRoleService.DeleteByUserAndApplicationAsync(userId, application.ApplicationId);
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
                filter = ua => ua.UserId == requestDto.UserId.Value;
            }

            if (requestDto.ApplicationId.HasValue)
            {
                filter = filter.AndAlso(x => x.ApplicationId == requestDto.ApplicationId.Value);
            }



            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                filter = filter.AndAlso(x => x.Application.Name.ToLower().Contains(requestDto.Filter.ToLower()) ||
                 x.Application.Code.ToLower().Contains(requestDto.Filter.ToLower()));
            }

            Func<IQueryable<UserApplication>, IOrderedQueryable<UserApplication>> orderBy = q => q.OrderByDescending(x => x.AssignedAt);

            var (items, totalRows) = await _userApplicationRepository.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                pageNumber: requestDto.PageNumber,
                pageSize: requestDto.PageSize,
                includeProperties: [x => x.User, y => y.Application]
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