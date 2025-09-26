using System.ComponentModel.DataAnnotations;

namespace SecurityMicroservice.Shared.Request.UserRole;

public class CreateUserRoleRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid RoleId { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}