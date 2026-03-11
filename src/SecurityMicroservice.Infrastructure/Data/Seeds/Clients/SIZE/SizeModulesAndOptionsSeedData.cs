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
        var inicioModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M001",
            Name = "Inicio",
            Description = "Panel principal del sistema Size",
            Icon = "home",
            Order = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var mantenimientoModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M002",
            Name = "Mantenimiento",
            Description = "Mantenimiento y configuración del sistema Size",
            Icon = "event_note",
            Order = 2,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var administracionModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M003",
            Name = "Administración",
            Description = "Administración general del sistema Size",
            Icon = "verified_user",
            Order = 3,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var operacionesModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M004",
            Name = "Operaciones",
            Description = "Gestión de operaciones Size",
            Icon = "settings",
            Order = 4,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var logisticaModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M005",
            Name = "Logística",
            Description = "Gestión logística del sistema Size",
            Icon = "local_shipping",
            Order = 5,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var reporteModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = sizeApp.ApplicationId,
            Code = "SIZE_M006",
            Name = "Reporte",
            Description = "Reportes del sistema Size",
            Icon = "assessment",
            Order = 6,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.Modules.AddRangeAsync(
            inicioModule,
            mantenimientoModule,
            administracionModule,
            operacionesModule,
            logisticaModule,
            reporteModule
        );
        await context.SaveChangesAsync();

        // === OPCIONES PARA SIZE ===

        // Opciones para Mantenimiento (SIZE_M002)
        var medidasCorporalesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = mantenimientoModule.ModuleId,
            Code = "SIZE_OP001",
            Name = "Medidas Corporales",
            Icon = "description",
            Route = "/secure/body-measurements",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var tallasOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = mantenimientoModule.ModuleId,
            Code = "SIZE_OP002",
            Name = "Tallas",
            Icon = "check_box",
            Route = "/secure/sizes",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var tipoNegocioOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = mantenimientoModule.ModuleId,
            Code = "SIZE_OP003",
            Name = "Tipo de Negocio",
            Icon = "edit_note",
            Route = "/secure/business-types",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var tipoProcesoOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = mantenimientoModule.ModuleId,
            Code = "SIZE_OP004",
            Name = "Tipo de Proceso",
            Icon = "settings",
            Route = "/secure/process-types",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var tiposMedidaProductoOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = mantenimientoModule.ModuleId,
            Code = "SIZE_OP005",
            Name = "Tipos de Medida de Producto",
            Icon = "list_alt",
            Route = "/secure/product-measurement-types",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var instruccionesEmbalajeOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = mantenimientoModule.ModuleId,
            Code = "SIZE_OP006",
            Name = "Instrucciones de Embalaje",
            Icon = "inventory_2",
            Route = "/secure/packaging-instructions",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Opciones para Administración (SIZE_M003)
        var personalOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = administracionModule.ModuleId,
            Code = "SIZE_OP007",
            Name = "Personal",
            Icon = "person",
            Route = "/secure/staff",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var clientesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = administracionModule.ModuleId,
            Code = "SIZE_OP008",
            Name = "Clientes",
            Icon = "groups",
            Route = "/secure/clients",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var tipoProductoOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = administracionModule.ModuleId,
            Code = "SIZE_OP009",
            Name = "Tipo de Producto",
            Icon = "label",
            Route = "/secure/product-types",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var ventasOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = administracionModule.ModuleId,
            Code = "SIZE_OP010",
            Name = "Ventas",
            Icon = "shopping_cart",
            Route = "/secure/sales",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Opciones para Operaciones (SIZE_M004)
        var asignacionSastresOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP011",
            Name = "Asignación de Sastres",
            Icon = "person_add",
            Route = "/secure/tailor-assignment",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var tomaMedidaOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP012",
            Name = "Toma de Medida",
            Icon = "groups",
            Route = "/secure/measurement-taking",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var controlCalidadOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP013",
            Name = "Control de Calidad",
            Icon = "security",
            Route = "/secure/quality-control",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var programaAjustesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = operacionesModule.ModuleId,
            Code = "SIZE_OP014",
            Name = "Programa de Ajustes",
            Icon = "settings",
            Route = "/secure/adjustment-program",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    var distribucionEntregaOption = new Option
            {
                OptionId = Guid.NewGuid(),
                ModuleId = logisticaModule.ModuleId,
                Code = "SIZE_OP015",
                Name = "Distribución y Entrega",
                Icon = "local_shipping",
                Route = "/secure/distribution-delivery",
                HttpMethod = "GET",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };


        // Opciones para Reporte (SIZE_M006)
        var conformidadOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = reporteModule.ModuleId,
            Code = "SIZE_OP016",
            Name = "Conformidad",
            Icon = "check",
            Route = "/secure/compliance",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Options.AddRangeAsync(
            medidasCorporalesOption,
            tallasOption,
            tipoNegocioOption,
            tipoProcesoOption,
            tiposMedidaProductoOption,
            instruccionesEmbalajeOption,
            personalOption,
            clientesOption,
            tipoProductoOption,
            ventasOption,
            asignacionSastresOption,
            tomaMedidaOption,
            controlCalidadOption,
            programaAjustesOption,
            distribucionEntregaOption,
            conformidadOption
        );
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Módulos y opciones de SIZE inicializados exitosamente:");
        Console.WriteLine("   - 6 Módulos creados");
        Console.WriteLine("   - 6 Opciones de Mantenimiento creadas");
        Console.WriteLine("   - 4 Opciones de Administración creadas");
        Console.WriteLine("   - 4 Opciones de Operaciones creadas");
        Console.WriteLine("   - 1 Opción de Logística creada");
        Console.WriteLine("   - 1 Opción de Reporte creada");
    }
}
