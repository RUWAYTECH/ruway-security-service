
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
            // ADMINISTRACIÓN
            { "SIZE_OP001", new[] { "APPADMIN", "AV", "JC" } }, // Gestión de personal
            // OPERACIONES
            { "SIZE_OP002", new[] { "APPADMIN", "AV", "JC" } }, // Gestión de Clientes
            { "SIZE_OP003", new[] { "APPADMIN", "AV", "JC" } }, // Asignación de Sastres
            { "SIZE_OP004", new[] { "APPADMIN", "AV", "JC", "AC" } }, // Toma de Medida
            { "SIZE_OP005", new[] { "APPADMIN", "AV", "JC", "AC" } }, // Clasificación de Talla
            { "SIZE_OP006", new[] { "APPADMIN", "AV", "JC", "AC", "GG" } }, // Control de Calidad
            { "SIZE_OP007", new[] { "APPADMIN", "AV", "JC" } }, // Programa de Ajustes
            { "SIZE_OP016", new[] { "APPADMIN", "AV", "JC" } }, // Packing
            // COMERCIAL Y LOGÍSTICA
            { "SIZE_OP008", new[] { "APPADMIN", "AV", "JC" } }, // Registro de Ventas
            { "SIZE_OP009", new[] { "APPADMIN", "AV", "JC" } }, // Distribución y Entrega
            // MANTENIMIENTO
            { "SIZE_OP010", new[] { "APPADMIN", "AV" } }, // Mantenimiento (si existe)
            // REPORTES
            { "SIZE_OP011", new[] { "APPADMIN", "AV", "JC", "AC", "GG", "GP" } }, // Reportes (si existe)
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
