using Microsoft.Extensions.Options;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Application.Services.Emails;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.IRepositories;
using SecurityMicroservice.Infrastructure.Services;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Application.Services;

public interface IAuthenticationService
{
    Task<User?> ValidateUserAsync(string username, string password);
    Task<User?> GetUserByIdAsync(Guid userId);
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
    private readonly IEmailService _emailService;
    private readonly WebAppSettings _webAppSettings;
    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IOptions<TokenConfiguration> tokenConfiguration,
        IEmailService emailService,
        IOptions<WebAppSettings> webAppSettings
        )
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenConfiguration = tokenConfiguration;
        _emailService = emailService;
        _webAppSettings = webAppSettings.Value;
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

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
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
                Success = false,
                Message = "El correo o usuario proporcionado no existe."
            };
        }

        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

        _userRepository.Update(user);

        await BuildSendEmail.ResetPasswordEmail(_emailService, user.Email, $"{user.FirstName} {user.LastName}", user.PasswordResetToken, _webAppSettings.Url);
  
        // For now, just return success message
        return new ForgotPasswordResponse
        {
            Success = true,
            Message = "Se ha enviado un correo electrónico con instrucciones para restablecer la contraseña."
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
                response.Message = "El token de restablecimiento de contraseña no es válido.";
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
            response.Message = ex.Message;
            return response;
        }
    }
}