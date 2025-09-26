using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.UserRole;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("api/user-roles")]
[Authorize]
public class UserRolesController : ControllerBase
{
    private readonly IUserRoleService _userRoleService;

    public UserRolesController(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    /// <summary>
    /// Obtiene todas las asignaciones de roles a usuarios con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponseDto<UserRoleDto>>> GetUserRoles(
        [FromQuery] UserRolePaginationRequestDto paginationRequestDto)
    {
        var result = await _userRoleService.GetPagedAsync(paginationRequestDto);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene una asignación específica por usuario y rol
    /// </summary>
    [HttpGet("{userId:guid}/{roleId:guid}")]
    public async Task<ActionResult<UserRoleDto>> GetUserRole(Guid userId, Guid roleId)
    {
        var userRole = await _userRoleService.GetByIdAsync(userId, roleId);
        if (userRole == null)
        {
            return NotFound();
        }

        return Ok(userRole);
    }

    /// <summary>
    /// Obtiene todos los roles asignados a un usuario específico
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<UserRoleDto>>> GetRolesByUser(Guid userId)
    {
        var userRoles = await _userRoleService.GetByUserIdAsync(userId);
        return Ok(userRoles);
    }

    /// <summary>
    /// Obtiene todos los usuarios asignados a un rol específico
    /// </summary>
    [HttpGet("role/{roleId:guid}")]
    public async Task<ActionResult<List<UserRoleDto>>> GetUsersByRole(Guid roleId)
    {
        var userRoles = await _userRoleService.GetByRoleIdAsync(roleId);
        return Ok(userRoles);
    }

    /// <summary>
    /// Asigna un rol a un usuario
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserRoleDto>> CreateUserRole(CreateUserRoleRequest request)
    {
        try
        {
            var userRole = await _userRoleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetUserRole), 
                new { userId = userRole.UserId, roleId = userRole.RoleId }, 
                userRole);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza una asignación de rol a usuario
    /// </summary>
    [HttpPut("{userId:guid}/{roleId:guid}")]
    public async Task<ActionResult<UserRoleDto>> UpdateUserRole(
        Guid userId, 
        Guid roleId, 
        UpdateUserRoleRequest request)
    {
        var userRole = await _userRoleService.UpdateAsync(userId, roleId, request);
        if (userRole == null)
        {
            return NotFound();
        }

        return Ok(userRole);
    }

    /// <summary>
    /// Elimina una asignación de rol a usuario
    /// </summary>
    [HttpDelete("{userId:guid}/{roleId:guid}")]
    public async Task<IActionResult> DeleteUserRole(Guid userId, Guid roleId)
    {
        var result = await _userRoleService.DeleteAsync(userId, roleId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}