using SecurityMicroservice.Domain.Common;

namespace SecurityMicroservice.Domain.Entities;

public class UserPermission: BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; } // Permisos temporales
    public Guid? GrantedByUserId { get; set; } // Quién otorgó el permiso
    public string? Reason { get; set; } = string.Empty; // Justificación del permiso
    public bool IsActive { get; set; } = true; // Para desactivar sin eliminar
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
    public User? GrantedByUser { get; set; } // Usuario que otorgó el permiso
}