
using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.SIZE;


public static class SizePermissionsSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        var sizeApp = await context.Applications.FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Size);
        if (sizeApp == null) return;

        // Obtener roles de SIZE
        var roles = await context.Roles
            .Where(r => r.ApplicationId == sizeApp.ApplicationId)
            .ToListAsync();
        if (!roles.Any()) return;

        // Obtener módulos y opciones de SIZE
        var modules = await context.Modules
            .Where(m => m.ApplicationId == sizeApp.ApplicationId)
            .ToListAsync();
        var options = await context.Options
            .Where(o => modules.Select(m => m.ModuleId).Contains(o.ModuleId))
            .ToListAsync();

        // Verificar si ya existen permisos para SIZE
        var existingPermissions = await context.Permissions
            .Where(p => p.Role != null && p.Role.ApplicationId == sizeApp.ApplicationId)
            .AnyAsync();
        if (existingPermissions)
        {
            Console.WriteLine("ℹ️ Los permisos de SIZE ya existen.");
            return;
        }

        Console.WriteLine("🚀 Inicializando permisos para SIZE...");

        // Matriz de permisos: código de opción -> roles
        var permissionMatrix = new Dictionary<string, string[]>
        {
            // MANTENIMIENTO (SIZE_M002)
            { "SIZE_OP001", new[] { "APPADMIN", "AV" } },         // Medidas Corporales
            { "SIZE_OP002", new[] { "APPADMIN", "AV" } },         // Tallas
            { "SIZE_OP003", new[] { "APPADMIN", "AV" } },         // Tipo de Negocio
            { "SIZE_OP004", new[] { "APPADMIN", "AV" } },         // Tipo de Proceso
            { "SIZE_OP005", new[] { "APPADMIN", "AV" } },         // Tipos de Medida de Producto
            { "SIZE_OP006", new[] { "APPADMIN", "AV" } },         // Instrucciones de Embalaje
            // ADMINISTRACIÓN (SIZE_M003)
            { "SIZE_OP007", new[] { "APPADMIN", "AV", "JC" } },   // Personal
            { "SIZE_OP008", new[] { "APPADMIN", "AV", "JC" } },   // Clientes
            { "SIZE_OP009", new[] { "APPADMIN", "AV" } },         // Tipo de Producto
            { "SIZE_OP010", new[] { "APPADMIN", "AV", "JC" } },   // Ventas
            // OPERACIONES (SIZE_M004)
            { "SIZE_OP011", new[] { "APPADMIN", "AV", "JC" } },   // Asignación de Sastres
            { "SIZE_OP012", new[] { "APPADMIN", "SASTRE" } }, // Toma de Medida
            { "SIZE_OP013", new[] { "APPADMIN", "JC", "AC", "GG" } },     // Control de Calidad
            { "SIZE_OP014", new[] { "APPADMIN", "AV", "JC" } },   // Programa de Ajustes
            // LOGÍSTICA (SIZE_M005)
            { "SIZE_OP015", new[] { "APPADMIN", "AV", "JC" } },   // Distribución y Entrega
            // REPORTE (SIZE_M006)
            { "SIZE_OP016", new[] { "APPADMIN", "AV", "JC", "AC", "GG" } }, // Conformidad
        };

        var permissions = new List<Permission>();

        // Asignar permisos cruzando roles, módulos y opciones
        foreach (var option in options)
        {
            if (permissionMatrix.TryGetValue(option.Code, out var allowedRoles))
            {
                foreach (var role in roles)
                {
                    if (allowedRoles.Contains(role.Code))
                    {
                        permissions.Add(new Permission
                        {
                            PermissionId = Guid.NewGuid(),
                            RoleId = role.RoleId,
                            OptionId = option.OptionId,
                            ActionCode = ActionCodes.Read,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Permisos de SIZE inicializados exitosamente:");
        Console.WriteLine($"   - {permissions.Count} permisos asignados a {roles.Count} roles");
        var permissionsByRole = permissions.GroupBy(p => p.RoleId);
        foreach (var roleGroup in permissionsByRole)
        {
            var roleName = roles.First(r => r.RoleId == roleGroup.Key).Code;
            Console.WriteLine($"   - {roleName}: {roleGroup.Count()} permisos");
        }
    }
}
