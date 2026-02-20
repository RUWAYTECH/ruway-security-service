using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.SIZE;

public static class SizeModulesAndOptionsSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)

    {
        var sizeApp = await context.Applications.FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Size);
        if (sizeApp == null) return;

        // Verificar si ya existen módulos para Size
        var existingModules = await context.Modules
            .Where(m => m.ApplicationId == sizeApp.ApplicationId)
            .AnyAsync();

        if (existingModules)
        {
            Console.WriteLine("ℹ️ Los módulos de SIZE ya existen.");
            return;
        }

        Console.WriteLine("🚀 Inicializando módulos y opciones para SIZE...");

        // === MÓDULOS PARA SIZE ===
        var adminModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M001",
            Name = "Administración",
            Description = "Configuración y administración del sistema Size",
            Icon = "admin_panel_settings",
            Order = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var operacionesModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M002",
            Name = "Operaciones",
            Description = "Gestión de operaciones Size",
            Icon = "build",
            Order = 2,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var comercialModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M003",
            Name = "Comercial y Logistica",
            Description = "Gestión comercial y logística Size",
            Icon = "local_shipping",
            Order = 3,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var mantenimientoModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M004",
            Name = "Mantenimiento",
            Description = "Mantenimiento del sistema Size",
            Icon = "settings",
            Order = 4,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var reportesModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M005",
            Name = "Reportes",
            Description = "Reportes del sistema Size",
            Icon = "assessment",
            Order = 5,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.Modules.AddRangeAsync(
            adminModule,
            operacionesModule,
            comercialModule,
            mantenimientoModule,
            reportesModule
        );
        await context.SaveChangesAsync();

        // === OPCIONES PARA SIZE ===
        // Administración
        var gestionPersonalOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = adminModule.ModuleId,
            Code = "SIZE_OP001",
            Name = "Gestión de personal",
            Icon = "people",
            Route = "/secure/personal",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Operaciones
        var gestionClientesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP002",
            Name = "Gestión de Clientes",
            Icon = "groups",
            Route = "/secure/clientes",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var asignacionSastresOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP003",
            Name = "Asignación de Sastres",
            Icon = "person_add",
            Route = "/secure/sastres",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var matrizMedidaOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP004",
            Name = "Toma de Medida",
            Icon = "table_chart",
            Route = "/secure/medidas",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var clasificacionTallaOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP005",
            Name = "Clasificación de Talla",
            Icon = "straighten",
            Route = "/secure/clasificacion-talla",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var controlCalidadOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP006",
            Name = "Control de Calidad",
            Icon = "check_circle",
            Route = "/secure/calidad",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var programaAjustesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP007",
            Name = "Programa de Ajustes",
            Icon = "tune",
            Route = "/secure/ajustes",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var packingOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP016",
            Name = "Packing",
            Icon = "inventory",
            Route = "/secure/packing",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Comercial y Logística
        var registroVentasOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = comercialModule.ModuleId,
            Code = "SIZE_OP008",
            Name = "Registro de Ventas",
            Icon = "description",
            Route = "/secure/ventas",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var distribucionEntregaOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = comercialModule.ModuleId,
            Code = "SIZE_OP009",
            Name = "Distribución y Entrega",
            Icon = "local_shipping",
            Route = "/secure/distribucion",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Options.AddRangeAsync(
            gestionPersonalOption,
            gestionClientesOption,
            asignacionSastresOption,
            matrizMedidaOption,
            clasificacionTallaOption,
            controlCalidadOption,
            programaAjustesOption,
            packingOption,
            registroVentasOption,
            distribucionEntregaOption
        );
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Módulos y opciones de SIZE inicializados exitosamente:");
        Console.WriteLine("   - 5 Módulos creados");
        Console.WriteLine("   - 10 Opciones creadas");
    }
}
