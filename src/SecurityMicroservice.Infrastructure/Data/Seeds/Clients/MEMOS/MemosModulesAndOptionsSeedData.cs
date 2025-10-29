using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.MEMOS;

public static class MemosModulesAndOptionsSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        var memosApp = await context.Applications.FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Memos);
        if (memosApp == null) return;

        // Verificar si ya existen módulos para MEMOS
        var existingModules = await context.Modules
            .Where(m => m.ApplicationId == memosApp.ApplicationId)
            .AnyAsync();

        if (existingModules)
        {
            Console.WriteLine("ℹ️ Los módulos de MEMOS ya existen.");
            return;
        }

        Console.WriteLine("🚀 Inicializando módulos y opciones para MEMOS...");

        // === MÓDULOS PARA MEMOS ===
        var memosInicioModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "M0001",
            Name = "Inicio",
            Description = "Dashboard principal del sistema",
            Icon = "home",
            Order = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var memosTrazabilidadModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "M0002",
            Name = "Trazabilidad de Documentos",
            Description = "Administración",
            Icon = "inventory",
            Order = 2,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var memosAdministracionModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "M0003",
            Name = "Administración",
            Description = "Módulo de administración del sistema",
            Icon = "admin_panel_settings",
            Order = 3,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.Modules.AddRangeAsync(
            memosInicioModule, 
            memosTrazabilidadModule, 
            memosAdministracionModule);

        await context.SaveChangesAsync();

        // === OPCIONES PARA MEMOS ===

        // Opciones para Inicio Module
        var memosDashboardOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosInicioModule.ModuleId,
            Code = "OP0001",
            Name = "Dashboard",
            Icon = "home",
            Route = "/secure/dashboard",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Opciones para Trazabilidad Module
        var memosMemorandumOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosTrazabilidadModule.ModuleId,
            Code = "OP0002",
            Name = "Memorándum",
            Icon = "description",
            Route = "/secure/memos",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosBandejaOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosTrazabilidadModule.ModuleId,
            Code = "OP0003",
            Name = "Bandeja de memorándum",
            Icon = "inbox",
            Route = "/secure/memos-inbox",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosReportesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosTrazabilidadModule.ModuleId,
            Code = "OP0004",
            Name = "Reportes",
            Icon = "bar_chart",
            Route = "/secure/reports",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Opciones para Administración Module
        var memosUsuariosOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosAdministracionModule.ModuleId,
            Code = "OP0005",
            Name = "Usuarios",
            Icon = "group",
            Route = "/secure/users",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosPerfilesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosAdministracionModule.ModuleId,
            Code = "OP0006",
            Name = "Perfiles",
            Icon = "manage_accounts",
            Route = "/secure/profile",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosEmpresaOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosAdministracionModule.ModuleId,
            Code = "OP0007",
            Name = "Empresa",
            Icon = "business",
            Route = "/secure/enterprise",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosTiendaOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosAdministracionModule.ModuleId,
            Code = "OP0008",
            Name = "Tienda",
            Icon = "store",
            Route = "/secure/store",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosEmpleadosOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosAdministracionModule.ModuleId,
            Code = "OP0009",
            Name = "Empleados",
            Icon = "badge",
            Route = "/secure/employee",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosSancionOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosAdministracionModule.ModuleId,
            Code = "OP0010",
            Name = "Tipo de sanción",
            Icon = "gavel",
            Route = "/secure/sanction",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosTemplateOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosAdministracionModule.ModuleId,
            Code = "OP0011",
            Name = "Template",
            Icon = "insert_page_break",
            Route = "/secure/templates",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosSecuenciaOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosAdministracionModule.ModuleId,
            Code = "OP0012",
            Name = "Secuencia",
            Icon = "web_stories",
            Route = "/secure/sequences",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Options.AddRangeAsync(
            memosDashboardOption,
            memosMemorandumOption,
            memosBandejaOption,
            memosReportesOption,
            memosUsuariosOption,
            memosPerfilesOption,
            memosEmpresaOption,
            memosTiendaOption,
            memosEmpleadosOption,
            memosSancionOption,
            memosTemplateOption,
            memosSecuenciaOption);

        await context.SaveChangesAsync();

        Console.WriteLine("✅ Módulos y opciones de MEMOS inicializados exitosamente:");
        Console.WriteLine("   - 3 Módulos creados");
        Console.WriteLine("   - 12 Opciones creadas");
    }
}