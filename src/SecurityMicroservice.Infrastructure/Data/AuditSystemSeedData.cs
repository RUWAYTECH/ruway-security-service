using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.Data;

public static class AuditSystemSeedData
{
    public static async Task InitializeAsync(SecurityDbContext context)
    {
        // Obtener la aplicación de Auditoría
        var auditoriaApp = await context.Applications
            .FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Auditoria);

        if (auditoriaApp == null)
        {
            Console.WriteLine("⚠️ La aplicación de Auditoría no existe. Ejecute primero el seed principal.");
            return;
        }

        // Obtener el rol AUDITOR_ADMIN
        var auditorAdminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Code == "AUDITOR_ADMIN" && r.ApplicationId == auditoriaApp.ApplicationId);

        if (auditorAdminRole == null)
        {
            Console.WriteLine("⚠️ El rol AUDITOR_ADMIN no existe. Ejecute primero el seed principal.");
            return;
        }

        // Verificar si ya existen módulos para Auditoría
        var existingModules = await context.Modules
            .Where(m => m.ApplicationId == auditoriaApp.ApplicationId)
            .AnyAsync();

        if (existingModules)
        {
            Console.WriteLine("ℹ️ Los módulos de Auditoría ya existen.");
            return; // Ya se han creado los módulos para Auditoría
        }

        Console.WriteLine("🚀 Iniciando seed de módulos, opciones y permisos para Auditoría...");

        // === MÓDULOS ===

        // Módulo Dashboard
        var auditoriaDashboardModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = auditoriaApp.ApplicationId,
            Code = "AUD_M001",
            Name = "Dashboard",
            Description = "Panel principal del sistema de auditoría",
            Icon = "dashboard",
            Order = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Módulo Administración
        var auditoriaAdminModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = auditoriaApp.ApplicationId,
            Code = "AUD_M002",
            Name = "Administración",
            Description = "Configuración y administración del sistema",
            Icon = "admin_panel_settings",
            Order = 2,
            CreatedAt = DateTime.UtcNow
        };

        // Módulo Auditoría
        var auditoriaAuditModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = auditoriaApp.ApplicationId,
            Code = "AUD_M003",
            Name = "Auditoría",
            Description = "Gestión de auditorías y procesos",
            Icon = "security",
            Order = 3,
            CreatedAt = DateTime.UtcNow
        };

        await context.Modules.AddRangeAsync(
            auditoriaDashboardModule,
            auditoriaAdminModule,
            auditoriaAuditModule
        );

        // === OPCIONES ===

        // Opciones para Dashboard
        var inicioOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = auditoriaDashboardModule.ModuleId,
            Code = "AUD_OP001",
            Name = "Inicio",
            Icon = "home",
            Route = "/secure/home",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Opciones para Administración
        var escalasEmpresaOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = auditoriaAdminModule.ModuleId,
            Code = "AUD_OP002",
            Name = "Escalas por empresa",
            Icon = "business",
            Route = "/secure/companyscales",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var gruposOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = auditoriaAdminModule.ModuleId,
            Code = "AUD_OP003",
            Name = "Grupos",
            Icon = "group_work",
            Route = "/secure/groups",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Opciones para Auditoría
        var gestionarAuditoriasOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = auditoriaAuditModule.ModuleId,
            Code = "AUD_OP004",
            Name = "Gestionar Auditorias",
            Icon = "assignment",
            Route = "/secure/manageaudits",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var reportesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = auditoriaAuditModule.ModuleId,
            Code = "AUD_OP005",
            Name = "Reportes",
            Icon = "assessment",
            Route = "/secure/reports",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Options.AddRangeAsync(
            inicioOption,
            escalasEmpresaOption,
            gruposOption,
            gestionarAuditoriasOption,
            reportesOption
        );

        // === PERMISOS PARA AUDITOR_ADMIN (acceso completo) ===

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
        AddPermissions(inicioOption.OptionId, ActionCodes.Read);

        // Permisos Administración (CRUD completo)
        AddPermissions(escalasEmpresaOption.OptionId, 
            ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);
        AddPermissions(gruposOption.OptionId, 
            ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);

        // Permisos Auditoría (CRUD completo)
        AddPermissions(gestionarAuditoriasOption.OptionId, 
            ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);
        AddPermissions(reportesOption.OptionId, 
            ActionCodes.Read, ActionCodes.Export);

        await context.Permissions.AddRangeAsync(permissions);

        await context.SaveChangesAsync();
        
        Console.WriteLine("✅ Seed de Auditoría completado exitosamente:");
        Console.WriteLine($"   - 3 Módulos creados");
        Console.WriteLine($"   - 5 Opciones creadas");
        Console.WriteLine($"   - {permissions.Count} Permisos asignados al rol AUDITOR_ADMIN");
    }
}