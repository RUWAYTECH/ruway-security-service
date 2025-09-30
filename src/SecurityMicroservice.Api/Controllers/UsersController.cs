using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.Common;
using SecurityMicroservice.Shared.DTOs;
using SecurityMicroservice.Shared.Request.User;

namespace SecurityMicroservice.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponseDto<UserDto>>> GetUsers([FromQuery] UserPaginationRequestDto paginationRequestDto)
    {
        var users = await _userService.GetPaged(paginationRequestDto);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetById(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(UserRequestDto request)
    {
        var result = await _userService.Create(request);
        if (result.IsValid)
        {
            return CreatedAtAction(nameof(CreateUser), new { id = result.Data.UserId }, result.Data);
        }
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UserRequestDto request)
    {
        var result = await _userService.Update(id, request);
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
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.Delete(id);
        if (result.IsValid)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}