using System.ComponentModel.DataAnnotations;

namespace SecurityMicroservice.Shared.Request.UserApplication;

public class UpdateUserApplicationRequest
{
    public bool? IsActive { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}