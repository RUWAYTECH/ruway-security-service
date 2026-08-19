namespace SecurityMicroservice.Shared.Request.Option;

public class CreateOptionRequest
{
    public Guid ModuleId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    // Opcional: si no se envía, se asigna el siguiente al mayor del módulo
    public int? Order { get; set; }
    public bool IsActive { get; set; } = true;
}