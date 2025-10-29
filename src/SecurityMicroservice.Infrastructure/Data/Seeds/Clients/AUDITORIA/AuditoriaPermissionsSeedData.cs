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

        // Obtener el rol AUDITOR_ADMIN
        var auditorAdminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Code == "AUDITOR_ADMIN" && r.ApplicationId == auditoriaApp.ApplicationId);

        if (auditorAdminRole == null) return;

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

        // Helper para crear permisos múltiples
        void AddPermissions(Guid optionId, params string[] actions)
        {
            foreach (var action in actions)
            {
                permissions.Add(new Permission
                {
                    PermissionId = Guid.NewGuid(),
                    RoleId = auditorAdminRole.RoleId,
                    OptionId = optionId,
                    ActionCode = action,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Permisos Dashboard
        if (inicioOption != null)
        {
            AddPermissions(inicioOption.OptionId, ActionCodes.Read);
        }

        // Permisos Administración (CRUD completo)
        if (escalasEmpresaOption != null)
        {
            AddPermissions(escalasEmpresaOption.OptionId, 
                ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);
        }

        if (gruposOption != null)
        {
            AddPermissions(gruposOption.OptionId, 
                ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);
        }

        // Permisos Auditoría (CRUD completo)
        if (gestionarAuditoriasOption != null)
        {
            AddPermissions(gestionarAuditoriasOption.OptionId, 
                ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);
        }

        if (reportesOption != null)
        {
            AddPermissions(reportesOption.OptionId, ActionCodes.Read);
        }

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Permisos de AUDITORIA inicializados exitosamente:");
        Console.WriteLine($"   - {permissions.Count} permisos asignados al rol AUDITOR_ADMIN");
    }
}