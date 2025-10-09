using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    /// <summary>
    /// Obtiene todas las aplicaciones con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponseDto<ApplicationDto>>> GetApplications(
        [FromQuery] PaginationRequestDto paginationRequestDto)
    {
        var result = await _applicationService.GetPagedAsync(paginationRequestDto);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene una aplicación por ID
    /// </summary>
    [HttpGet("{applicationId:guid}")]
    public async Task<ActionResult<ApplicationDto>> GetApplication(Guid applicationId)
    {
        var result = await _applicationService.GetByIdAsync(applicationId);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        
        if (result.Data == null)
        {
            return NotFound("Aplicación no encontrada.");
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene una aplicación por código
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<ApplicationDto>> GetApplicationByCode(string code)
    {
        var result = await _applicationService.GetByCodeAsync(code);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        
        if (result.Data == null)
        {
            return NotFound("Aplicación no encontrada.");
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Crea una nueva aplicación
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> CreateApplication(CreateApplicationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _applicationService.CreateAsync(request);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }

        return CreatedAtAction(
            nameof(GetApplication), 
            new { applicationId = result.Data!.ApplicationId }, 
            result.Data);
    }

    /// <summary>
    /// Actualiza una aplicación existente
    /// </summary>
    [HttpPut("{applicationId:guid}")]
    public async Task<ActionResult<ApplicationDto>> UpdateApplication(
        Guid applicationId, 
        UpdateApplicationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _applicationService.UpdateAsync(applicationId, request);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }

        if (result.Data == null)
        {
            return NotFound("Aplicación no encontrada.");
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Elimina (desactiva) una aplicación
    /// </summary>
    [HttpDelete("{applicationId:guid}")]
    public async Task<IActionResult> DeleteApplication(Guid applicationId)
    {
        var result = await _applicationService.DeleteAsync(applicationId);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }

        return Ok(new { Message = "Aplicación desactivada correctamente." });
    }
}