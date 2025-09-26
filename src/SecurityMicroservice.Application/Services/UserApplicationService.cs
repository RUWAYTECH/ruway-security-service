using AutoMapper;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.UserApplication;
using SecurityMicroservice.Shared.Response.Common;
using System.Linq.Expressions;

namespace SecurityMicroservice.Application.Services;

public interface IUserApplicationService
{
    Task<List<UserApplicationDto>> GetAllAsync();
    Task<UserApplicationDto?> GetByIdAsync(Guid userId, Guid applicationId);
    Task<List<UserApplicationDto>> GetByUserIdAsync(Guid userId);
    Task<List<UserApplicationDto>> GetByApplicationIdAsync(Guid applicationId);
    Task<UserApplicationDto> CreateAsync(CreateUserApplicationRequest request);
    Task<UserApplicationDto?> UpdateAsync(Guid userId, Guid applicationId, UpdateUserApplicationRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid applicationId);
    Task<ResponseDto<PaginationResponseDto<UserApplicationDto>>> GetPagedAsync(UserApplicationPaginationRequestDto requestDto);
}

public class UserApplicationService : IUserApplicationService
{
    private readonly IUserApplicationRepository _userApplicationRepository;
    private readonly IMapper _mapper;

    public UserApplicationService(
        IUserApplicationRepository userApplicationRepository,
        IMapper mapper)
    {
        _userApplicationRepository = userApplicationRepository;
        _mapper = mapper;
    }

    public async Task<List<UserApplicationDto>> GetAllAsync()
    {
        var userApplications = await _userApplicationRepository.GetAllAsync();
        return _mapper.Map<List<UserApplicationDto>>(userApplications);
    }

    public async Task<UserApplicationDto?> GetByIdAsync(Guid userId, Guid applicationId)
    {
        var userApplication = await _userApplicationRepository.GetByIdAsync(userId, applicationId);
        return userApplication != null ? _mapper.Map<UserApplicationDto>(userApplication) : null;
    }

    public async Task<List<UserApplicationDto>> GetByUserIdAsync(Guid userId)
    {
        var userApplications = await _userApplicationRepository.GetByUserIdAsync(userId);
        return _mapper.Map<List<UserApplicationDto>>(userApplications);
    }

    public async Task<List<UserApplicationDto>> GetByApplicationIdAsync(Guid applicationId)
    {
        var userApplications = await _userApplicationRepository.GetByApplicationIdAsync(applicationId);
        return _mapper.Map<List<UserApplicationDto>>(userApplications);
    }

    public async Task<UserApplicationDto> CreateAsync(CreateUserApplicationRequest request)
    {
        // Verificar si ya existe la asignación
        var exists = await _userApplicationRepository.ExistsAsync(request.UserId, request.ApplicationId);
        if (exists)
        {
            throw new InvalidOperationException("El usuario ya tiene asignada esta aplicación.");
        }

        var userApplication = new UserApplication
        {
            UserId = request.UserId,
            ApplicationId = request.ApplicationId,
            IsActive = request.IsActive,
            Notes = request.Notes,
            AssignedAt = DateTime.UtcNow
        };

        var created = await _userApplicationRepository.CreateAsync(userApplication);
        return _mapper.Map<UserApplicationDto>(created);
    }

    public async Task<UserApplicationDto?> UpdateAsync(Guid userId, Guid applicationId, UpdateUserApplicationRequest request)
    {
        var userApplication = await _userApplicationRepository.GetByIdAsync(userId, applicationId);
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

        var updated = await _userApplicationRepository.UpdateAsync(userApplication);
        return _mapper.Map<UserApplicationDto>(updated);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid applicationId)
    {
        var userApplication = await _userApplicationRepository.GetByIdAsync(userId, applicationId);
        if (userApplication == null) return false;

        await _userApplicationRepository.DeleteAsync(userId, applicationId);
        return true;
    }

    public async Task<ResponseDto<PaginationResponseDto<UserApplicationDto>>> GetPagedAsync(UserApplicationPaginationRequestDto requestDto)
    {
        var response = ResponseDto.Create<PaginationResponseDto<UserApplicationDto>>();
        try
        {
            Expression<Func<UserApplication, bool>>? filter = null;

            if (requestDto.UserId.HasValue)
            {
                filter = ua => ua.UserId == requestDto.UserId.Value;
            }

            if (requestDto.ApplicationId.HasValue)
            {
                var applicationFilter = new Func<Expression<Func<UserApplication, bool>>, Expression<Func<UserApplication, bool>>>(
                    existing => existing == null 
                        ? ua => ua.ApplicationId == requestDto.ApplicationId.Value
                        : ua => existing.Compile()(ua) && ua.ApplicationId == requestDto.ApplicationId.Value);
                filter = applicationFilter(filter);
            }

            if (!string.IsNullOrEmpty(requestDto.ApplicationCode))
            {
                var appCodeFilter = new Func<Expression<Func<UserApplication, bool>>, Expression<Func<UserApplication, bool>>>(
                    existing => existing == null 
                        ? ua => ua.Application.Code == requestDto.ApplicationCode
                        : ua => existing.Compile()(ua) && ua.Application.Code == requestDto.ApplicationCode);
                filter = appCodeFilter(filter);
            }

            if (requestDto.IsActive.HasValue)
            {
                var activeFilter = new Func<Expression<Func<UserApplication, bool>>, Expression<Func<UserApplication, bool>>>(
                    existing => existing == null 
                        ? ua => ua.IsActive == requestDto.IsActive.Value
                        : ua => existing.Compile()(ua) && ua.IsActive == requestDto.IsActive.Value);
                filter = activeFilter(filter);
            }

            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                var searchFilter = requestDto.Filter.ToLower();
                var textFilter = new Func<Expression<Func<UserApplication, bool>>, Expression<Func<UserApplication, bool>>>(
                    existing => existing == null 
                        ? ua => ua.User.Username.ToLower().Contains(searchFilter) ||
                                ua.Application.Name.ToLower().Contains(searchFilter) ||
                                ua.Application.Code.ToLower().Contains(searchFilter)
                        : ua => existing.Compile()(ua) && 
                                (ua.User.Username.ToLower().Contains(searchFilter) ||
                                 ua.Application.Name.ToLower().Contains(searchFilter) ||
                                 ua.Application.Code.ToLower().Contains(searchFilter)));
                filter = textFilter(filter);
            }

            Func<IQueryable<UserApplication>, IOrderedQueryable<UserApplication>> orderBy = q => q.OrderByDescending(x => x.AssignedAt);

            var (items, totalRows) = await _userApplicationRepository.GetPagedAsync(
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
}