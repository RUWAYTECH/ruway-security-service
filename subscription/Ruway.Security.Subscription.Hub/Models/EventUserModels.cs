namespace Ruway.Security.Subscription.Hub.Models;

/// <summary>
/// Resultado de la operación de creación de usuario
/// </summary>
public class UserCreationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? GeneratedUsername { get; set; }
    public string? GeneratedPassword { get; set; }
    public Exception? Exception { get; set; }

    public static UserCreationResult CreateSuccess(Guid userId, string username, string password)
    {
        return new UserCreationResult
        {
            Success = true,
            UserId = userId,
            GeneratedUsername = username,
            GeneratedPassword = password,
            Message = "Usuario creado exitosamente"
        };
    }

    public static UserCreationResult CreateError(string message, Exception? exception = null)
    {
        return new UserCreationResult
        {
            Success = false,
            Message = message,
            Exception = exception
        };
    }
}