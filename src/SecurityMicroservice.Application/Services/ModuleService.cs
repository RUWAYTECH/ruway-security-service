using AutoMapper;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Extensions;
using SecurityMicroservice.Shared.Response.Common;
using System.Linq.Expressions;

namespace SecurityMicroservice.Application.Services;

public class ModuleService : IModuleService
{
    private readonly IModuleRepository _moduleRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IMapper _mapper;

    public ModuleService(
        IModuleRepository moduleRepository,
        IApplicationRepository applicationRepository,
        IMapper mapper)
    {
        _moduleRepository = moduleRepository;
        _applicationRepository = applicationRepository;
        _mapper = mapper;
    }

    public async Task<List<ModuleManagementDto>> GetAllAsync()
    {
        var modules = await _moduleRepository.GetAllAsync();
        return _mapper.Map<List<ModuleManagementDto>>(modules);
    }

    public async Task<ResponseDto<ModuleManagementDto>> GetByIdAsync(Guid moduleId)
    {
        var result = ResponseDto.Create<ModuleManagementDto>();
        try
        {
            var module = await _moduleRepository.GetByKeyAsync(moduleId);
            if (module != null)
            {
                result.Data = _mapper.Map<ModuleManagementDto>(module);
            }
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<ModuleManagementDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<ModuleManagementDto>> GetByCodeAsync(string code, Guid applicationId)
    {
        var result = ResponseDto.Create<ModuleManagementDto>();
        try
        {
            var module = await _moduleRepository.GetFirstOrDefaultAsync(
                filter: x => x.Code == code && x.ApplicationId == applicationId);
            if (module != null)
            {
                result.Data = _mapper.Map<ModuleManagementDto>(module);
            }
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<ModuleManagementDto>(ex.Message);
        }
        return result;
    }

    public async Task<List<ModuleManagementDto>> GetByApplicationIdAsync(Guid applicationId)
    {
        var allModules = await _moduleRepository.GetAllAsync();
        var modules = allModules.Where(x => x.ApplicationId == applicationId).OrderBy(x => x.Order).ToList();
        return _mapper.Map<List<ModuleManagementDto>>(modules);
    }

    public async Task<ResponseDto<ModuleManagementDto>> CreateAsync(CreateModuleRequest request)
    {
        var result = ResponseDto.Create<ModuleManagementDto>();
        try
        {
            // Validar que la aplicación existe y está activa
            var application = await _applicationRepository.GetByKeyAsync(request.ApplicationId);
            if (application == null || !application.IsActive)
            {
                return ResponseDto.Error<ModuleManagementDto>("La aplicación especificada no existe o no está activa.");
            }

            // Validar que no existe un módulo con el mismo código en la aplicación
            var existingModule = await _moduleRepository.GetFirstOrDefaultAsync(
                filter: x => x.Code == request.Code && x.ApplicationId == request.ApplicationId);
            if (existingModule != null)
            {
                return ResponseDto.Error<ModuleManagementDto>("Ya existe un módulo con el mismo código en esta aplicación.");
            }

            var module = new Domain.Entities.Module
            {
                ModuleId = Guid.NewGuid(),
                ApplicationId = request.ApplicationId,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon,
                Order = request.Order,
                CreatedAt = DateTime.UtcNow
            };

            _moduleRepository.Insert(module);
            result.Data = _mapper.Map<ModuleManagementDto>(module);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<ModuleManagementDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<ModuleManagementDto>> UpdateAsync(Guid moduleId, UpdateModuleRequest request)
    {
        var result = ResponseDto.Create<ModuleManagementDto>();
        try
        {
            var module = await _moduleRepository.GetByKeyAsync(moduleId);
            if (module == null)
            {
                return ResponseDto.Error<ModuleManagementDto>("Módulo no encontrado.");
            }

            // Validar código único si se está cambiando
            if (!string.IsNullOrEmpty(request.Code) && request.Code != module.Code)
            {
                var existingModule = await _moduleRepository.GetFirstOrDefaultAsync(
                    filter: x => x.Code == request.Code && x.ApplicationId == module.ApplicationId && x.ModuleId != moduleId);
                if (existingModule != null)
                {
                    return ResponseDto.Error<ModuleManagementDto>("Ya existe un módulo con el mismo código en esta aplicación.");
                }
                module.Code = request.Code;
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                module.Name = request.Name;
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                module.Description = request.Description;
            }

            if (!string.IsNullOrEmpty(request.Icon))
            {
                module.Icon = request.Icon;
            }

            if (request.Order.HasValue)
            {
                module.Order = request.Order.Value;
            }

            module.UpdatedAt = DateTime.UtcNow;

            _moduleRepository.Update(module);
            result.Data = _mapper.Map<ModuleManagementDto>(module);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error<ModuleManagementDto>(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto> DeleteAsync(Guid moduleId)
    {
        var result = ResponseDto.Create();
        try
        {
            var module = await _moduleRepository.GetByKeyAsync(moduleId);
            if (module == null)
            {
                return ResponseDto.Error("Módulo no encontrado.");
            }

            // Hard delete para módulos (o puedes implementar soft delete agregando IsActive a la entidad)
            _moduleRepository.Delete(module);
        }
        catch (Exception ex)
        {
            result = ResponseDto.Error(ex.Message);
        }
        return result;
    }

    public async Task<ResponseDto<PaginationResponseDto<ModuleManagementDto>>> GetPagedAsync(ModuleFilterRequestDto requestDto)
    {
        var response = ResponseDto.Create<PaginationResponseDto<ModuleManagementDto>>();
        try
        {
            Expression<Func<Domain.Entities.Module, bool>>? filter = null;

            if (requestDto.ApplicationId != Guid.Empty)
            {
                filter = filter.AndAlso(m => m.ApplicationId == requestDto.ApplicationId);
            }
            // Filtro de búsqueda por texto
            if (!string.IsNullOrEmpty(requestDto.Filter))
            {
                var searchFilter = requestDto.Filter.ToLower();
                filter = filter.AndAlso(module => module.Name.ToLower().Contains(searchFilter) ||
                               module.Code.ToLower().Contains(searchFilter) ||
                               module.Description.ToLower().Contains(searchFilter) ||
                               module.Application.Name.ToLower().Contains(searchFilter) ||
                               module.Application.Code.ToLower().Contains(searchFilter));
            }

            // Ordenamiento por defecto: por aplicación y luego por orden
            Func<IQueryable<Domain.Entities.Module>, IOrderedQueryable<Domain.Entities.Module>> orderBy = 
                q => q.OrderBy(x => x.Application.Name).ThenBy(x => x.Order);

            var (items, totalRows) = await _moduleRepository.GetPagedAsync(
                filter: filter,
                orderBy: orderBy,
                pageNumber: requestDto.PageNumber,
                pageSize: requestDto.PageSize,
                includeProperties: [x => x.Application]
            );

            response.Data = new PaginationResponseDto<ModuleManagementDto>
            {
                Items = _mapper.Map<IEnumerable<ModuleManagementDto>>(items),
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };
        }
        catch (Exception ex)
        {
            response = ResponseDto.Error<PaginationResponseDto<ModuleManagementDto>>(ex.Message);
        }
        return response;
    }
}