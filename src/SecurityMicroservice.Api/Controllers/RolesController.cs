using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Obtiene todos los roles con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponseDto<RoleDto>>> GetRoles(
        [FromQuery] PaginationRequestDto paginationRequestDto)
    {
        var result = await _roleService.GetPagedAsync(paginationRequestDto);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene un rol por ID
    /// </summary>
    [HttpGet("{roleId:guid}")]
    public async Task<ActionResult<RoleDto>> GetRole(Guid roleId)
    {
        var result = await _roleService.GetByIdAsync(roleId);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        
        if (result.Data == null)
        {
            return NotFound("Rol no encontrado.");
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene un rol por código y aplicación
    /// </summary>
    [HttpGet("by-code/{code}/application/{applicationId:guid}")]
    public async Task<ActionResult<RoleDto>> GetRoleByCode(string code, Guid applicationId)
    {
        var result = await _roleService.GetByCodeAsync(code, applicationId);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }
        
        if (result.Data == null)
        {
            return NotFound("Rol no encontrado.");
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene todos los roles de una aplicación específica
    /// </summary>
    [HttpGet("application/{applicationId:guid}")]
    public async Task<ActionResult<List<RoleDto>>> GetRolesByApplication(Guid applicationId)
    {
        var roles = await _roleService.GetByApplicationIdAsync(applicationId);
        return Ok(roles);
    }

    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _roleService.CreateAsync(request);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }

        return CreatedAtAction(
            nameof(GetRole), 
            new { roleId = result.Data!.RoleId }, 
            result.Data);
    }

    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    [HttpPut("{roleId:guid}")]
    public async Task<ActionResult<RoleDto>> UpdateRole(
        Guid roleId, 
        UpdateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _roleService.UpdateAsync(roleId, request);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }

        if (result.Data == null)
        {
            return NotFound("Rol no encontrado.");
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Elimina (desactiva) un rol
    /// </summary>
    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> DeleteRole(Guid roleId)
    {
        var result = await _roleService.DeleteAsync(roleId);
        if (!result.IsValid)
        {
            return BadRequest(result.Messages);
        }

        return Ok(new { Message = "Rol desactivado correctamente." });
    }
}