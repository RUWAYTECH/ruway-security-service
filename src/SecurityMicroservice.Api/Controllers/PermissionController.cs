using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.Request.Permission;

namespace SecurityMicroservice.Api.Controllers;
[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }
    [HttpPost]
    public async Task<IActionResult> CreatePermission([FromBody] PermissionRequestDto requestDto)
    {
        var result = await _permissionService.Create(requestDto);
        if (result.IsValid)
        {
            return CreatedAtAction(nameof(CreatePermission), new { id = result.Data.PermissionId }, result.Data);
        }
        return BadRequest(result);
    }

    [HttpGet()]
    public async Task<IActionResult> GetPagedPermissions([FromQuery] PaginationRequestDto paginationRequestDto)
    {
        var result = await _permissionService.GetPaged(paginationRequestDto);
        if (result.IsValid)
        {
            return Ok(result.Data);
        }
        return BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPermissionById(Guid id)
    {
        var result = await _permissionService.GetById(id);
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] PermissionRequestDto requestDto)
    {
        var result = await _permissionService.Update(id, requestDto);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        var result = await _permissionService.Delete(id);
        if (result.IsValid)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

}
