using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class User: BaseEntity
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public Guid? EmployeeId { get; set; } // Reference to master microservice
    public DateTime? LastLoginAt { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpires { get; set; }    
    // Navigation properties
    public ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public ICollection<Audit> Audits { get; set; } = new List<Audit>();
}

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Locked = 3,
    Suspended = 4
}