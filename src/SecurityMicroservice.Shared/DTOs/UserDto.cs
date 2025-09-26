namespace SecurityMicroservice.Shared.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<ApplicationDto> Applications { get; set; } = new();
    public List<RoleDto> Roles { get; set; } = new();
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? EmployeeId { get; set; }
    public List<Guid> ApplicationIds { get; set; } = new();
}

public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Status { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? EmployeeId { get; set; }
    public List<Guid>? ApplicationIds { get; set; }
}