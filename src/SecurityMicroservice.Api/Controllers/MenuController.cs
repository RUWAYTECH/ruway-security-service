using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.Services;
using SecurityMicroservice.Shared.DTOs;
using System.Security.Claims;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("api/menus")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    /// <summary>
    /// Obtiene el menú completo del usuario autenticado para una aplicación específica
    /// </summary>
    /// <param name="applicationCode">Código de la aplicación (ej: AUDITORIA, MEMOS, SECURITY)</param>
    /// <returns>Menú estructurado por módulos y opciones con permisos del usuario</returns>
    [HttpGet("{applicationCode}")]
    public async Task<ActionResult<MenuDto>> GetMenuByApplication(string applicationCode)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var menu = await _menuService.GetMenuByApplicationAsync(applicationCode.ToUpperInvariant(), userId);
        
        if (menu == null)
        {
            return NotFound($"No menu found for application '{applicationCode}' or user has no permissions");
        }

        return Ok(menu);
    }

    /// <summary>
    /// Obtiene todos los menús disponibles para el usuario autenticado
    /// </summary>
    /// <returns>Lista de menús de todas las aplicaciones donde el usuario tiene permisos</returns>
    [HttpGet]
    public async Task<ActionResult<List<MenuDto>>> GetAllUserMenus()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var menus = await _menuService.GetAllUserMenusAsync(userId);
        
        return Ok(menus);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                         User.FindFirst("sub")?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }
}