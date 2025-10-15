using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Extensions;
using SecurityMicroservice.Shared.Request.Option;
using SecurityMicroservice.Shared.Response.Common;
using System.Linq.Expressions;

namespace SecurityMicroservice.Application.Services;

public class OptionService : IOptionService
{
    private readonly IOptionRepository _optionRepository;
    private readonly IModuleRepository _moduleRepository;
    
    private readonly IMapper _mapper;

    public OptionService(
        IOptionRepository optionRepository,
        IModuleRepository moduleRepository,
        IMapper mapper)
    {
        _optionRepository = optionRepository;
        _moduleRepository = moduleRepository;
        _mapper = mapper;
    }

    public async Task<List<OptionDto>> GetAllAsync()
    {
        var options = await _optionRepository.GetAllAsync(
            includeProperties: o => o.Module);
        return _mapper.Map<List<OptionDto>>(options);
    }

    public async Task<OptionDto?> GetByIdAsync(Guid optionId)
    {
        var option = await _optionRepository.GetByKeyAsync(optionId);
        return option != null ? _mapper.Map<OptionDto>(option) : null;
    }

    public async Task<List<OptionDto>> GetByModuleIdAsync(Guid moduleId)
    {
        var options = await _optionRepository.GetByModuleIdAsync(moduleId);
        return _mapper.Map<List<OptionDto>>(options);
    }

    public async Task<List<OptionDto>> GetByApplicationCodeAsync(string applicationCode)
    {
        var options = await _optionRepository.GetByApplicationCodeAsync(applicationCode);
        return _mapper.Map<List<OptionDto>>(options);
    }

    public async Task<OptionDto?> GetByCodeAsync(string code)
    {
        var option = await _optionRepository.GetByCodeAsync(code);
        return option != null ? _mapper.Map<OptionDto>(option) : null;
    }

    public async Task<ResponseDto<OptionDto>> CreateAsync(CreateOptionRequest request)
    {
        var result = ResponseDto.Create<OptionDto>();
        try
        {
            // Verificar que el módulo existe
            var module = await _moduleRepository.GetByKeyAsync(request.ModuleId);
            if (module == null)
            {
                return ResponseDto.Error<OptionDto>("El módulo especificado no existe.");
            }

            // Verificar que no existe otra opción con el mismo código
            var existingOption = await _optionRepository.GetByCodeAsync(request.Code);
            if (existingOption != null)
            {
                return ResponseDto.Error<OptionDto>($"Ya existe una opción con el código '{request.Code}'.");
            }

            var option = _mapper.Map<Option>(request);
            option.OptionId = Guid.NewGuid();
            option.CreatedAt = DateTime.UtcNow;

            _optionRepository.Insert(option);

            // Cargar la opción con sus relaciones
            var createdOption = await _optionRepository.GetByCodeAsync(option.Code);

            result.Data = _mapper.Map<OptionDto>(createdOption);
        }
        catch (Exception ex)
        {
            return ResponseDto.Error<OptionDto>($"Error al crear la opción: {ex.Message}");
        }
        return result;
    }

    public async Task<ResponseDto<OptionDto>> UpdateAsync(Guid optionId, UpdateOptionRequest request)
    {
        var result = ResponseDto.Create<OptionDto>();
        try
        {
            var option = await _optionRepository.GetByKeyAsync(optionId);
            if (option == null)
            {
                return ResponseDto.Error<OptionDto>("Opción no encontrada.");
            }

            // Verificar código único si se está actualizando
            if (!string.IsNullOrEmpty(request.Code) && request.Code != option.Code)
            {
                var existingOption = await _optionRepository.GetByCodeAsync(request.Code);
                if (existingOption != null)
                {
                    return ResponseDto.Error<OptionDto>($"Ya existe una opción con el código '{request.Code}'.");
                }
            }

            // Actualizar solo los campos proporcionados
            if (!string.IsNullOrEmpty(request.Code))
                option.Code = request.Code;
            if (!string.IsNullOrEmpty(request.Name))
                option.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Icon))
                option.Icon = request.Icon;
            if (!string.IsNullOrEmpty(request.Route))
                option.Route = request.Route;
            if (!string.IsNullOrEmpty(request.HttpMethod))
                option.HttpMethod = request.HttpMethod;
            if (request.IsActive.HasValue)
                option.IsActive = request.IsActive.Value;

            option.UpdatedAt = DateTime.UtcNow;

            _optionRepository.Update(option);

            // Cargar la opción actualizada con sus relaciones
            var updatedOption = await _optionRepository.GetByCodeAsync(option.Code);

            result.Data = _mapper.Map<OptionDto>(updatedOption);
        }
        catch (Exception ex)
        {
            return ResponseDto.Error<OptionDto>($"Error al actualizar la opción: {ex.Message}");
        }
        return result;
    }

    public async Task<ResponseDto> DeleteAsync(Guid optionId)
    {
        var result = ResponseDto.Create();
        try
        {
            var option = await _optionRepository.GetByKeyAsync(optionId);
            if (option == null)
            {
                return ResponseDto.Error("Opción no encontrada.");
            }

            _optionRepository.Delete(option);
        }
        catch (Exception ex)
        {
            return ResponseDto.Error($"Error al eliminar la opción: {ex.Message}");
        }
        return result;
    }

    public async Task<ResponseDto<PaginationResponseDto<OptionDto>>> GetPagedAsync(OptionPaginationRequestDto requestDto)
    {
        var result = ResponseDto.Create<PaginationResponseDto<OptionDto>>();
        try
        {
            Expression<Func<Option, bool>>? filter = a=>a.IsActive;

           
                filter = filter.AndAlso(o => o.ModuleId == requestDto.ModuleId);
         

            // Ordenamiento
            Func<IQueryable<Option>, IOrderedQueryable<Option>> orderBy = q => 
                q.OrderBy(o => o.Module.Application.Code)
                 .ThenBy(o => o.Module.Order)
                 .ThenBy(o => o.Name);

            var (items, totalRows) = await _optionRepository.GetPagedAsync(
                filter,
                orderBy,
                requestDto.PageNumber,
                requestDto.PageSize,
                o => o.Module.Application);

            var optionDtos = _mapper.Map<List<OptionDto>>(items);

            var paginationResponse = new PaginationResponseDto<OptionDto>
            {
                Items = optionDtos,
                TotalCount = totalRows,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize
            };

            result.Data = paginationResponse;
        }
        catch (Exception ex)
        {
            return ResponseDto.Error<PaginationResponseDto<OptionDto>>($"Error al obtener las opciones paginadas: {ex.Message}");
        }
        return result;
    }

    
}