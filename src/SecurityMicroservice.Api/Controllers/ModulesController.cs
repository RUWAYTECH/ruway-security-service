using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("api/modules")]
[Authorize]
public class ModulesController : ControllerBase
{
    private readonly IModuleService _moduleService;

    public ModulesController(IModuleService moduleService)
    {
        _moduleService = moduleService;
    }

    /// <summary>
    /// Obtiene todos los módulos con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponseDto<ModuleManagementDto>>> GetModules(
        [FromQuery] ModuleFilterRequestDto paginationRequestDto)
    {
        var result = await _moduleService.GetPagedAsync(paginationRequestDto);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        return Ok(result.Data);
    }

 
    /// <summary>
    /// Obtiene un módulo por ID
    /// </summary>
    [HttpGet("{moduleId:guid}")]
    public async Task<ActionResult<ModuleManagementDto>> GetModule(Guid moduleId)
    {
        var result = await _moduleService.GetByIdAsync(moduleId);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        
        if (result.Data == null)
        {
            return NotFound("Módulo no encontrado.");
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene un módulo por código y aplicación
    /// </summary>
    [HttpGet("by-code/{code}/application/{applicationId:guid}")]
    public async Task<ActionResult<ModuleManagementDto>> GetModuleByCode(string code, Guid applicationId)
    {
        var result = await _moduleService.GetByCodeAsync(code, applicationId);
        if (!result.IsValid)
        {
            return BadRequest(result);
        }
        
        if (result.Data == null)
        {
            return NotFound("Módulo no encontrado.");
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene todos los módulos de una aplicación específica
    /// </summary>
    [HttpGet("application/{applicationId:guid}")]
    public async Task<ActionResult<List<ModuleManagementDto>>> GetModulesByApplication(Guid applicationId)
    {
        var modules = await _moduleService.GetByApplicationIdAsync(applicationId);
        return Ok(modules);
    }

    /// <summary>
    /// Crea un nuevo módulo
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ModuleManagementDto>> CreateModule(CreateModuleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _moduleService.CreateAsync(request);
        if (!result.IsValid)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetModule), 
            new { moduleId = result.Data!.ModuleId }, 
            result.Data);
    }

    /// <summary>
    /// Actualiza un módulo existente
    /// </summary>
    [HttpPut("{moduleId:guid}")]
    public async Task<ActionResult<ModuleManagementDto>> UpdateModule(
        Guid moduleId, 
        UpdateModuleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _moduleService.UpdateAsync(moduleId, request);
        if (!result.IsValid)
        {
            return BadRequest(result);
        }

        if (result.Data == null)
        {
            return NotFound("Módulo no encontrado.");
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Elimina un módulo
    /// </summary>
    [HttpDelete("{moduleId:guid}")]
    public async Task<IActionResult> DeleteModule(Guid moduleId)
    {
        var result = await _moduleService.DeleteAsync(moduleId);
        if (!result.IsValid)
        {
            return BadRequest(result);
        }

        return Ok(new { Message = "Módulo eliminado correctamente." });
    }
}