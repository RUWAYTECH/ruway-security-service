namespace SecurityMicroservice.Shared.Request.Option;

public class UpdateOptionRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public string? HttpMethod { get; set; }
    public bool? IsActive { get; set; }
}