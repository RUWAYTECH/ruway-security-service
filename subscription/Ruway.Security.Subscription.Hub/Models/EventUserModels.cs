namespace Ruway.Security.Subscription.Hub.Models;

/// <summary>
/// Modelo para la creación de usuarios desde eventos
/// </summary>
public class EventUserCreationModel
{
    public Guid? EmployeeId { get; set; }
    public Guid? PeopleId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string UserType { get; set; } = string.Empty; // "Employee" o "People"
    public bool IsExternal { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Relationship { get; set; } // Para beneficiarios
    public bool IsActive { get; set; } = true;
}

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