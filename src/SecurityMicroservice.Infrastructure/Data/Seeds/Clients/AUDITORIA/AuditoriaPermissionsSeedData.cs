using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.AUDITORIA;

public static class AuditoriaPermissionsSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        var auditoriaApp = await context.Applications.FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Auditoria);
        if (auditoriaApp == null) return;

        // Obtener todos los roles de AUDITORIA
        var roles = await context.Roles
            .Where(r => r.ApplicationId == auditoriaApp.ApplicationId)
            .ToListAsync();

        if (!roles.Any()) return;

        // Verificar si ya existen permisos para Auditoría
        var existingPermissions = await context.Permissions
            .Where(p => p.Role != null && p.Role.ApplicationId == auditoriaApp.ApplicationId)
            .AnyAsync();

        if (existingPermissions)
        {
            Console.WriteLine("ℹ️ Los permisos de AUDITORIA ya existen.");
            return;
        }

        Console.WriteLine("🚀 Inicializando permisos para AUDITORIA...");

        // Obtener opciones de AUDITORIA
        var inicioOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "AUD_OP001");
        var escalasEmpresaOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "AUD_OP002");
        var gruposOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "AUD_OP003");
        var gestionarAuditoriasOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "AUD_OP004");
        var reportesOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "AUD_OP005");

        var permissions = new List<Permission>();

        // Helper para crear permisos
        void AddPermission(Guid roleId, Guid optionId, string actionCode)
        {
            permissions.Add(new Permission
            {
                PermissionId = Guid.NewGuid(),
                RoleId = roleId,
                OptionId = optionId,
                ActionCode = actionCode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Configurar permisos por rol
        foreach (var role in roles)
        {
            switch (role.Code)
            {
                case "A001": // AUDITOR_ADMIN - Acceso completo a todas las opciones con GET
                    if (inicioOption != null)
                        AddPermission(role.RoleId, inicioOption.OptionId, ActionCodes.Read);
                    if (escalasEmpresaOption != null)
                        AddPermission(role.RoleId, escalasEmpresaOption.OptionId, ActionCodes.Read);
                    if (gruposOption != null)
                        AddPermission(role.RoleId, gruposOption.OptionId, ActionCodes.Read);
                    if (gestionarAuditoriasOption != null)
                        AddPermission(role.RoleId, gestionarAuditoriasOption.OptionId, ActionCodes.Read);
                    if (reportesOption != null)
                        AddPermission(role.RoleId, reportesOption.OptionId, ActionCodes.Read);
                    break;

                case "A002": // AUDITOR_SENIOR - Acceso a AUD_OP004 con GET
                case "A003": // AUDITOR_JUNIOR - Acceso a AUD_OP004 con GET
                case "A004": // CONSULTOR - Acceso a AUD_OP004 con GET
                case "A005": // REVISOR - Acceso a AUD_OP004 con GET
                    if (gestionarAuditoriasOption != null)
                        AddPermission(role.RoleId, gestionarAuditoriasOption.OptionId, ActionCodes.Read);
                    break;

                case "A006": // OBSERVADOR - Acceso a AUD_OP004 y AUD_OP005 con GET
                    if (gestionarAuditoriasOption != null)
                        AddPermission(role.RoleId, gestionarAuditoriasOption.OptionId, ActionCodes.Read);
                    if (reportesOption != null)
                        AddPermission(role.RoleId, reportesOption.OptionId, ActionCodes.Read);
                    break;
            }
        }

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Permisos de AUDITORIA inicializados exitosamente:");
        Console.WriteLine($"   - {permissions.Count} permisos asignados a {roles.Count} roles");
        
        // Mostrar resumen por rol
        var permissionsByRole = permissions.GroupBy(p => p.RoleId);
        foreach (var roleGroup in permissionsByRole)
        {
            var roleName = roles.First(r => r.RoleId == roleGroup.Key).Code;
            Console.WriteLine($"   - {roleName}: {roleGroup.Count()} permisos");
        }
    }
}