namespace SecurityMicroservice.Shared.DTOs;

/// <summary>
/// Request model for forgot password functionality
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Username or email for password recovery
    /// </summary>
    public string Username { get; set; } = string.Empty;
}

/// <summary>
/// Response model for forgot password operation
/// </summary>
public class ForgotPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request model for password reset functionality
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Password reset token
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Confirmation of the new password
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response model for password reset operation
/// </summary>
public class ResetPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Standard API response model for generic operations
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

/// <summary>
/// OAuth authorization request model
/// </summary>
public class AuthorizeRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public string? RedirectUri { get; set; }
    public string? Scope { get; set; }
    public string? State { get; set; }
    public string? CodeChallenge { get; set; }
    public string? CodeChallengeMethod { get; set; }
}