using System.ComponentModel.DataAnnotations;

namespace SecurityMicroservice.Shared.Request.UserPermission;

public class UpdateUserPermissionRequest
{
    public DateTime? ExpiresAt { get; set; }
    
    [StringLength(500)]
    public string? Reason { get; set; }
    
    public bool? IsActive { get; set; }
}