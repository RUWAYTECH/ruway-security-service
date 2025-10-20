using System.ComponentModel.DataAnnotations;

namespace SecurityMicroservice.Shared.Request.UserApplication;

public class CreateUserApplicationRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid ApplicationId { get; set; }

    public List<Guid> RoleIds { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}