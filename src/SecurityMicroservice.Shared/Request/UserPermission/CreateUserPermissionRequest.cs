using System.ComponentModel.DataAnnotations;

namespace SecurityMicroservice.Shared.Request.UserPermission;

public class CreateUserPermissionRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid PermissionId { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    [StringLength(500)]
    public string? Reason { get; set; }
}