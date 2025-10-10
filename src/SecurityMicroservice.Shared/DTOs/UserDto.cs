namespace SecurityMicroservice.Shared.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
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