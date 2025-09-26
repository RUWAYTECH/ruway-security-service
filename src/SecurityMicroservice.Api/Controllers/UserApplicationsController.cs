using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.UserApplication;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("api/user-applications")]
[Authorize]
public class UserApplicationsController : ControllerBase
{
    private readonly IUserApplicationService _userApplicationService;

    public UserApplicationsController(IUserApplicationService userApplicationService)
    {
        _userApplicationService = userApplicationService;
    }

    /// <summary>
    /// Obtiene todas las asignaciones de aplicaciones a usuarios con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponseDto<UserApplicationDto>>> GetUserApplications(
        [FromQuery] UserApplicationPaginationRequestDto paginationRequestDto)
    {
        var result = await _userApplicationService.GetPagedAsync(paginationRequestDto);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene una asignación específica por usuario y aplicación
    /// </summary>
    [HttpGet("{userId:guid}/{applicationId:guid}")]
    public async Task<ActionResult<UserApplicationDto>> GetUserApplication(Guid userId, Guid applicationId)
    {
        var userApplication = await _userApplicationService.GetByIdAsync(userId, applicationId);
        if (userApplication == null)
        {
            return NotFound();
        }

        return Ok(userApplication);
    }

    /// <summary>
    /// Obtiene todas las aplicaciones asignadas a un usuario específico
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<UserApplicationDto>>> GetApplicationsByUser(Guid userId)
    {
        var userApplications = await _userApplicationService.GetByUserIdAsync(userId);
        return Ok(userApplications);
    }

    /// <summary>
    /// Obtiene todos los usuarios asignados a una aplicación específica
    /// </summary>
    [HttpGet("application/{applicationId:guid}")]
    public async Task<ActionResult<List<UserApplicationDto>>> GetUsersByApplication(Guid applicationId)
    {
        var userApplications = await _userApplicationService.GetByApplicationIdAsync(applicationId);
        return Ok(userApplications);
    }

    /// <summary>
    /// Asigna una aplicación a un usuario
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserApplicationDto>> CreateUserApplication(CreateUserApplicationRequest request)
    {
        try
        {
            var userApplication = await _userApplicationService.CreateAsync(request);
            return CreatedAtAction(nameof(GetUserApplication), 
                new { userId = userApplication.UserId, applicationId = userApplication.ApplicationId }, 
                userApplication);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza una asignación de aplicación a usuario
    /// </summary>
    [HttpPut("{userId:guid}/{applicationId:guid}")]
    public async Task<ActionResult<UserApplicationDto>> UpdateUserApplication(
        Guid userId, 
        Guid applicationId, 
        UpdateUserApplicationRequest request)
    {
        var userApplication = await _userApplicationService.UpdateAsync(userId, applicationId, request);
        if (userApplication == null)
        {
            return NotFound();
        }

        return Ok(userApplication);
    }

    /// <summary>
    /// Elimina una asignación de aplicación a usuario
    /// </summary>
    [HttpDelete("{userId:guid}/{applicationId:guid}")]
    public async Task<IActionResult> DeleteUserApplication(Guid userId, Guid applicationId)
    {
        var result = await _userApplicationService.DeleteAsync(userId, applicationId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}