using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.UserPermission;
using System.Security.Claims;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("api/user-permissions")]
[Authorize]
public class UserPermissionsController : ControllerBase
{
    private readonly IUserPermissionService _userPermissionService;

    public UserPermissionsController(IUserPermissionService userPermissionService)
    {
        _userPermissionService = userPermissionService;
    }

    /// <summary>
    /// Obtiene todas las asignaciones de permisos directos a usuarios con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponseDto<UserPermissionDto>>> GetUserPermissions(
        [FromQuery] UserPermissionPaginationRequestDto paginationRequestDto)
    {
        var result = await _userPermissionService.GetPagedAsync(paginationRequestDto);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene una asignación específica por usuario y permiso
    /// </summary>
    [HttpGet("{userId:guid}/{permissionId:guid}")]
    public async Task<ActionResult<UserPermissionDto>> GetUserPermission(Guid userId, Guid permissionId)
    {
        var userPermission = await _userPermissionService.GetByIdAsync(userId, permissionId);
        if (userPermission == null)
        {
            return NotFound();
        }

        return Ok(userPermission);
    }

    /// <summary>
    /// Obtiene todos los permisos directos asignados a un usuario específico
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<UserPermissionDto>>> GetPermissionsByUser(Guid userId)
    {
        var userPermissions = await _userPermissionService.GetByUserIdAsync(userId);
        return Ok(userPermissions);
    }

    /// <summary>
    /// Obtiene todos los usuarios que tienen un permiso específico asignado directamente
    /// </summary>
    [HttpGet("permission/{permissionId:guid}")]
    public async Task<ActionResult<List<UserPermissionDto>>> GetUsersByPermission(Guid permissionId)
    {
        var userPermissions = await _userPermissionService.GetByPermissionIdAsync(permissionId);
        return Ok(userPermissions);
    }

    /// <summary>
    /// Asigna un permiso directamente a un usuario
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserPermissionDto>> CreateUserPermission(CreateUserPermissionRequest request)
    {
        try
        {
            // Obtener el ID del usuario que está otorgando el permiso
            var grantedByUserId = GetCurrentUserId();
            
            var userPermission = await _userPermissionService.CreateAsync(request, grantedByUserId);
            return CreatedAtAction(nameof(GetUserPermission), 
                new { userId = userPermission.UserId, permissionId = userPermission.PermissionId }, 
                userPermission);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza una asignación de permiso directo a usuario
    /// </summary>
    [HttpPut("{userId:guid}/{permissionId:guid}")]
    public async Task<ActionResult<UserPermissionDto>> UpdateUserPermission(
        Guid userId, 
        Guid permissionId, 
        UpdateUserPermissionRequest request)
    {
        var userPermission = await _userPermissionService.UpdateAsync(userId, permissionId, request);
        if (userPermission == null)
        {
            return NotFound();
        }

        return Ok(userPermission);
    }

    /// <summary>
    /// Elimina una asignación de permiso directo a usuario
    /// </summary>
    [HttpDelete("{userId:guid}/{permissionId:guid}")]
    public async Task<IActionResult> DeleteUserPermission(Guid userId, Guid permissionId)
    {
        var result = await _userPermissionService.DeleteAsync(userId, permissionId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                         User.FindFirst("sub")?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}