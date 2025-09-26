using System.ComponentModel.DataAnnotations;

namespace SecurityMicroservice.Shared.Request.UserRole;

public class UpdateUserRoleRequest
{
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime? RevokedAt { get; set; }
}