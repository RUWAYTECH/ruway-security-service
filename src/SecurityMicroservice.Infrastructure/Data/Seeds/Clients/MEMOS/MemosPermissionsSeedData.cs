using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.MEMOS;

public static class MemosPermissionsSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        var memosApp = await context.Applications.FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Memos);
        if (memosApp == null) return;

        // Obtener roles de MEMOS
        var memoSysAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Code == "R005" && r.ApplicationId == memosApp.ApplicationId); // SYSADMIN
        var memoRRHHRole = await context.Roles.FirstOrDefaultAsync(r => r.Code == "R004" && r.ApplicationId == memosApp.ApplicationId); // RRHH
        var memoAdministradorRole = await context.Roles.FirstOrDefaultAsync(r => r.Code == "R001" && r.ApplicationId == memosApp.ApplicationId); // Administrador
        var memoSupervisorRole = await context.Roles.FirstOrDefaultAsync(r => r.Code == "R002" && r.ApplicationId == memosApp.ApplicationId); // Supervisor
        var memoJefaturaRole = await context.Roles.FirstOrDefaultAsync(r => r.Code == "R006" && r.ApplicationId == memosApp.ApplicationId); // Jefatura
        var memoEmpleadoRole = await context.Roles.FirstOrDefaultAsync(r => r.Code == "R003" && r.ApplicationId == memosApp.ApplicationId); // Empleado

        if (memoSysAdminRole == null) return;

        // Verificar si ya existen permisos
        var existingPermissions = await context.Permissions
            .Where(p => p.Role != null && p.Role.ApplicationId == memosApp.ApplicationId)
            .AnyAsync();

        if (existingPermissions)
        {
            Console.WriteLine("ℹ️ Los permisos de MEMOS ya existen.");
            return;
        }

        Console.WriteLine("🚀 Inicializando permisos para MEMOS...");

        // Obtener opciones de MEMOS
        var memosDashboardOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0001");
        var memosMemorandumOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0002");
        var memosBandejaOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0003");
        var memosReportesOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0004");
        var memosUsuariosOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0005");
        var memosPerfilesOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0006");
        var memosEmpresaOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0007");
        var memosTiendaOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0008");
        var memosEmpleadosOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0009");
        var memosSancionOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0010");
        var memosTemplateOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0011");
        var memosSecuenciaOption = await context.Options.FirstOrDefaultAsync(o => o.Code == "OP0012");

        var permissions = new List<Permission>();

        // Permisos para SYSADMIN (acceso completo a todo)
        if (memoSysAdminRole != null)
        {
            var sysAdminOptions = new[]
            {
                memosDashboardOption, memosMemorandumOption, memosBandejaOption, memosReportesOption,
                memosUsuariosOption, memosPerfilesOption, memosEmpresaOption, memosTiendaOption,
                memosEmpleadosOption, memosSancionOption, memosTemplateOption, memosSecuenciaOption
            };

            foreach (var option in sysAdminOptions.Where(o => o != null))
            {
                permissions.Add(new Permission
                {
                    PermissionId = Guid.NewGuid(),
                    RoleId = memoSysAdminRole.RoleId,
                    OptionId = option!.OptionId,
                    ActionCode = ActionCodes.Read,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Permisos para RRHH (mismos que SYSADMIN excepto Template y Secuencia)
        if (memoRRHHRole != null)
        {
            var rrhhOptions = new[]
            {
                memosDashboardOption, memosMemorandumOption, memosBandejaOption, memosReportesOption,
                memosUsuariosOption, memosPerfilesOption, memosEmpresaOption, memosTiendaOption,
                memosEmpleadosOption, memosSancionOption
            };

            foreach (var option in rrhhOptions.Where(o => o != null))
            {
                permissions.Add(new Permission
                {
                    PermissionId = Guid.NewGuid(),
                    RoleId = memoRRHHRole.RoleId,
                    OptionId = option!.OptionId,
                    ActionCode = ActionCodes.Read,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Permisos para Administrador (acceso a bandeja)
        if (memoAdministradorRole != null && memosBandejaOption != null)
        {
            permissions.Add(new Permission
            {
                PermissionId = Guid.NewGuid(),
                RoleId = memoAdministradorRole.RoleId,
                OptionId = memosBandejaOption.OptionId,
                ActionCode = ActionCodes.Read,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Permisos para Supervisor (acceso a bandeja)
        if (memoSupervisorRole != null && memosBandejaOption != null)
        {
            permissions.Add(new Permission
            {
                PermissionId = Guid.NewGuid(),
                RoleId = memoSupervisorRole.RoleId,
                OptionId = memosBandejaOption.OptionId,
                ActionCode = ActionCodes.Read,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Permisos para Jefatura (acceso a bandeja)
        if (memoJefaturaRole != null && memosBandejaOption != null)
        {
            permissions.Add(new Permission
            {
                PermissionId = Guid.NewGuid(),
                RoleId = memoJefaturaRole.RoleId,
                OptionId = memosBandejaOption.OptionId,
                ActionCode = ActionCodes.Read,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Permisos para Empleado (acceso limitado a bandeja)
        if (memoEmpleadoRole != null && memosBandejaOption != null)
        {
            permissions.Add(new Permission
            {
                PermissionId = Guid.NewGuid(),
                RoleId = memoEmpleadoRole.RoleId,
                OptionId = memosBandejaOption.OptionId,
                ActionCode = ActionCodes.Read,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Permisos de MEMOS inicializados exitosamente:");
        Console.WriteLine($"   - {permissions.Count} permisos creados");
    }
}