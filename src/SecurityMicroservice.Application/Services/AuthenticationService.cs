using Microsoft.Extensions.Options;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Infrastructure.Services;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Application.Services;

public interface IAuthenticationService
{
    Task<User?> ValidateUserAsync(string username, string password);
    Task<TokenResponse> GenerateTokenResponseAsync(User user);
    Task UpdateLastLoginAsync(Guid userId);
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IOptions<TokenConfiguration> _tokenConfiguration;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IOptions<TokenConfiguration> tokenConfiguration)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenConfiguration = tokenConfiguration;
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
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ExpiresIn = _tokenConfiguration.Value.AccessTokenLifetimeMinutes
        };
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            _userRepository.Update(user);
        }
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            // Don't reveal if user exists for security
            return new ForgotPasswordResponse
            {
                Success = true,
                Message = "If the username exists, a password reset email has been sent."
            };
        }

          user.PasswordResetToken = _passwordService.GenerateRandomToken();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            _userRepository.Update(user);

        // For now, just return success message
        return new ForgotPasswordResponse
        {
            Success = true,
            Message = "If the username exists, a password reset email has been sent."
        };
    }

    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var response = new ResetPasswordResponse { Success = false };
        try
        {

            var user = await _userRepository.GetByTokenAsync(request.Token);

            if (user == null)
            {
                return response;
            }

            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            _userRepository.Update(user);

            //  await _auditService.LogAsync("User", AuditAction.PasswordChange, user.Id);

            response.Success = true;
            return response;
        }
        catch (Exception ex)
        {

            return response;
        }
    }
}