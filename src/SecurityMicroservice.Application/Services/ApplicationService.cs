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

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IMapper _mapper;

    private readonly IHttpContextAccessor _httpContextAccessor;
    public ApplicationService(
        IApplicationRepository applicationRepository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _applicationRepository = applicationRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<ApplicationDto>> GetAllAsync()
    {
        var applications = await _applicationRepository.GetAsync(a => a.IsActive);
        return _mapper.Map<List<ApplicationDto>>(applications);
    }

    public async Task<ResponseDto<ApplicationDto>> GetByIdAsync(Guid applicationId)
    {
        var result = ResponseDto.Create<ApplicationDto>();
        try
        {
            var application = await _applicationRepository.GetByKeyAsync(applicationId);
            if (application != null)
            {
                result.Data = _mapper.Map<ApplicationDto>(application);
            }
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<ApplicationDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<ApplicationDto>> GetByCodeAsync(string code)
    {
        var result = ResponseDto.Create<ApplicationDto>();
        try
        {
            var application = await _applicationRepository.GetFirstOrDefaultAsync(
                filter: x => x.Code == code && x.IsActive);
            if (application != null)
            {
                result.Data = _mapper.Map<ApplicationDto>(application);
            }
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<ApplicationDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<ApplicationDto>> CreateAsync(CreateApplicationRequest request)
    {
        var result = ResponseDto.Create<ApplicationDto>();
        try
        {
            // Validar que no existe una aplicación con el mismo código
            var existingApp = await _applicationRepository.GetFirstOrDefaultAsync(
                filter: x => x.Code == request.Code);
            if (existingApp != null)
            {
                return ResponseDto.Error<ApplicationDto>("Ya existe una aplicación con el mismo código.");
            }

            var application = new Domain.Entities.Application
            {
                ApplicationId = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name,
                BaseUrl = request.BaseUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _applicationRepository.Insert(application);
            result.Data = _mapper.Map<ApplicationDto>(application);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<ApplicationDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<ApplicationDto>> UpdateAsync(Guid applicationId, UpdateApplicationRequest request)
    {
        var result = ResponseDto.Create<ApplicationDto>();
        try
        {
            var application = await _applicationRepository.GetByKeyAsync(applicationId);
            if (application == null)
            {
                return ResponseDto.Error<ApplicationDto>("Aplicación no encontrada.");
            }

            // Validar código único si se está cambiando
            if (!string.IsNullOrEmpty(request.Code) && request.Code != application.Code)
            {
                var existingApp = await _applicationRepository.GetFirstOrDefaultAsync(
                    filter: x => x.Code == request.Code && x.ApplicationId != applicationId);
                if (existingApp != null)
                {
                    return ResponseDto.Error<ApplicationDto>("Ya existe una aplicación con el mismo código.");
                }
                application.Code = request.Code;
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                application.Name = request.Name;
            }

            if (!string.IsNullOrEmpty(request.BaseUrl))
            {
                application.BaseUrl = request.BaseUrl;
            }

            if (request.IsActive.HasValue)
            {
                application.IsActive = request.IsActive.Value;
            }

            application.UpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(application);
            result.Data = _mapper.Map<ApplicationDto>(application);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<ApplicationDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto> DeleteAsync(Guid applicationId)
    {
        var result = ResponseDto.Create();
        try
        {
            var application = await _applicationRepository.GetByKeyAsync(applicationId);
            if (application == null)
            {
                return ResponseDto.Error("Aplicación no encontrada.");
            }

            // Soft delete - marcar como inactiva
            application.IsActive = false;
            application.UpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(application);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<PaginationResponseDto<ApplicationDto>>> GetPagedAsync(PaginationRequestDto requestDto)
    {
        var response = ResponseDto.Create<PaginationResponseDto<ApplicationDto>>();
        try
        {
            var currentUser = _httpContextAccessor.CurrentUser();
            if (currentUser.IsSuperAdmin == false && currentUser.IsAppAdmin == false)
            {
                response.Data = new PaginationResponseDto<ApplicationDto>
                {
                    Items = new List<ApplicationDto>(),
                    TotalCount = 0,
                    PageNumber = requestDto.PageNumber,
                    PageSize = requestDto.PageSize
                };
            }

            Expression<Func<Domain.Entities.Application, bool>>? filter = a => a.IsActive;
            
            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                var searchFilter = requestDto.Filter.ToLower();
                filter = app => app.Name.ToLower().Contains(searchFilter) ||
                               app.Code.ToLower().Contains(searchFilter) ||
                               app.BaseUrl.ToLower().Contains(searchFilter);
            }

            if (currentUser.IsAppAdmin)
            {
                var userApplicationCodes = currentUser.Roles?
                    .Where(a => a.Code == RoleCodes.ApplicationAdmin)
                    .Select(r => r.ApplicationCode)
                    .Distinct()
                    .ToList() ?? new List<string>();
                 filter = filter.AndAlso(app => userApplicationCodes.Contains(app.Code));
            }

            
            // Ordenamiento por defecto: por fecha de creación descendente
            Func<IQueryable<Domain.Entities.Application>, IOrderedQueryable<Domain.Entities.Application>> orderBy =
                q => q.OrderByDescending(x => x.CreatedAt);

            var (items, totalRows) = await _applicationRepository.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                pageNumber: requestDto.PageNumber,
                pageSize: requestDto.PageSize
            );

            response.Data = new PaginationResponseDto<ApplicationDto>
            {
                Items = _mapper.Map<IEnumerable<ApplicationDto>>(items),
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };
        }
        catch (Exception ex)
        {
            response = ResponseDto.Error<PaginationResponseDto<ApplicationDto>>(ex.Message);
        }
        return response;
    }
}