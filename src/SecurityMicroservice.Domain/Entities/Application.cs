namespace SecurityMicroservice.Domain.Entities;

public class Application
{
    public Guid ApplicationId { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty; // e.g., AUDITORIA, MEMOS
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<UserApplication> UserApplications { get; set; } = new List<UserApplication>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<Option> Options { get; set; } = new List<Option>();
}