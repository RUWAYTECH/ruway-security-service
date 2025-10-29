using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.AUDITORIA;

public static class AuditoriaModulesAndOptionsSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        var auditoriaApp = await context.Applications.FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Auditoria);
        if (auditoriaApp == null) return;

        // Verificar si ya existen módulos para Auditoría
        var existingModules = await context.Modules
            .Where(m => m.ApplicationId == auditoriaApp.ApplicationId)
            .AnyAsync();

        if (existingModules)
        {
            Console.WriteLine("ℹ️ Los módulos de AUDITORIA ya existen.");
            return;
        }

        Console.WriteLine("🚀 Inicializando módulos y opciones para AUDITORIA...");

        // === MÓDULOS PARA AUDITORIA ===

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
            CreatedAt = DateTime.UtcNow,
            IsActive = true
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
            CreatedAt = DateTime.UtcNow,
            IsActive = true
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
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.Modules.AddRangeAsync(
            auditoriaDashboardModule,
            auditoriaAdminModule,
            auditoriaAuditModule
        );

        await context.SaveChangesAsync();

        // === OPCIONES PARA AUDITORIA ===

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
            Name = "Configuración de grupos",
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

        await context.SaveChangesAsync();

        Console.WriteLine("✅ Módulos y opciones de AUDITORIA inicializados exitosamente:");
        Console.WriteLine("   - 3 Módulos creados");
        Console.WriteLine("   - 5 Opciones creadas");
    }
}