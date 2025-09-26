using System.ComponentModel.DataAnnotations;

namespace SecurityMicroservice.Shared.Request.UserApplication;

public class CreateUserApplicationRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid ApplicationId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [StringLength(500)]
    public string? Notes { get; set; }
}