namespace SecurityMicroservice.Domain.Entities;

public class Option
{
    public Guid OptionId { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Application Application { get; set; } = null!;
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}