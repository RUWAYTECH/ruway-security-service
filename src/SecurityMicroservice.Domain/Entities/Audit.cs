namespace SecurityMicroservice.Domain.Entities;

public class Audit
{
    public Guid AuditId { get; set; } = Guid.NewGuid();
    public string Entity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Data { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}