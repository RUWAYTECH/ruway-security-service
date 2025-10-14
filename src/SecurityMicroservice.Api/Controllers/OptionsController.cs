using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.Option;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("api/options")]
[Authorize]
public class OptionsController : ControllerBase
{
    private readonly IOptionService _optionService;

    public OptionsController(IOptionService optionService)
    {
        _optionService = optionService;
    }

    /// <summary>
    /// Obtiene todas las opciones con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponseDto<OptionDto>>> GetOptions(
        [FromQuery] OptionPaginationRequestDto paginationRequestDto)
    {
        var result = await _optionService.GetPagedAsync(paginationRequestDto);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene una opción específica por ID
    /// </summary>
    [HttpGet("{optionId:guid}")]
    public async Task<ActionResult<OptionDto>> GetOption(Guid optionId)
    {
        var option = await _optionService.GetByIdAsync(optionId);
        if (option == null)
        {
            return NotFound();
        }

        return Ok(option);
    }

    /// <summary>
    /// Obtiene una opción específica por código
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<OptionDto>> GetOptionByCode(string code)
    {
        var option = await _optionService.GetByCodeAsync(code);
        if (option == null)
        {
            return NotFound();
        }

        return Ok(option);
    }

    /// <summary>
    /// Obtiene todas las opciones de un módulo específico
    /// </summary>
    [HttpGet("module/{moduleId:guid}")]
    public async Task<ActionResult<List<OptionDto>>> GetOptionsByModule(Guid moduleId)
    {
        var options = await _optionService.GetByModuleIdAsync(moduleId);
        return Ok(options);
    }

    /// <summary>
    /// Obtiene todas las opciones de una aplicación específica por código
    /// </summary>
    [HttpGet("application/{applicationCode}")]
    public async Task<ActionResult<List<OptionDto>>> GetOptionsByApplication(string applicationCode)
    {
        var options = await _optionService.GetByApplicationCodeAsync(applicationCode);
        return Ok(options);
    }

    /// <summary>
    /// Crea una nueva opción
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OptionDto>> CreateOption(CreateOptionRequest request)
    {
        var result = await _optionService.CreateAsync(request);
        if (result.IsValid)
        {
            return CreatedAtAction(nameof(GetOption), new { optionId = result.Data!.OptionId }, result.Data);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Actualiza una opción existente
    /// </summary>
    [HttpPut("{optionId:guid}")]
    public async Task<ActionResult<OptionDto>> UpdateOption(
        Guid optionId, 
        UpdateOptionRequest request)
    {
        var result = await _optionService.UpdateAsync(optionId, request);
        if (result.IsValid)
        {
            if (result.Data == null)
            {
                return NotFound();
            }
            return Ok(result.Data);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Elimina una opción
    /// </summary>
    [HttpDelete("{optionId:guid}")]
    public async Task<IActionResult> DeleteOption(Guid optionId)
    {
        var result = await _optionService.DeleteAsync(optionId);
        if (result.IsValid)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}