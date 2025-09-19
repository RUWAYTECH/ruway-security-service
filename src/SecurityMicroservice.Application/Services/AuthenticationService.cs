using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Repositories;
using SecurityMicroservice.Infrastructure.Services;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Application.Services;

public interface IAuthenticationService
{
    Task<User?> ValidateUserAsync(string username, string password);
    Task<TokenResponse> GenerateTokenResponseAsync(User user);
    Task UpdateLastLoginAsync(Guid userId);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        
        if (user == null || user.Status != UserStatus.Active)
        {
            return null;
        }

        if (!_passwordService.VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }

    public async Task<TokenResponse> GenerateTokenResponseAsync(User user)
    {
        var scopes = await _userRepository.GetUserApplicationScopesAsync(user.UserId);
        var roles = await _userRepository.GetUserRolesAsync(user.UserId);
        var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId);

        return new TokenResponse
        {
            Scope = string.Join(" ", scopes),
            Roles = roles,
            Permissions = permissions,
            EmployeeId = user.EmployeeId,
            ExpiresIn = 3600 // 1 hour
        };
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }
    }
}