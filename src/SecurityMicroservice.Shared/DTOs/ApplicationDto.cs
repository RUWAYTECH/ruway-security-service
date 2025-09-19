namespace SecurityMicroservice.Shared.DTOs;

public class ApplicationDto
{
    public Guid ApplicationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateApplicationRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}

public class UpdateApplicationRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? BaseUrl { get; set; }
    public bool? IsActive { get; set; }
}