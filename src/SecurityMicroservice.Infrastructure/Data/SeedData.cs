using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Services;

namespace SecurityMicroservice.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(SecurityDbContext context, IPasswordService passwordService)
    {
        if (await context.Users.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Create applications
        var auditoriaApp = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Code = ApplicationCodes.Auditoria,
            Name = "Sistema de Auditoría",
            Icon = "security",
            BaseUrl = "https://auditoria.company.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosApp = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Code = ApplicationCodes.Memos,
            Name = "Trazabilidad de documentos",
            Icon = "description",
            BaseUrl = "https://memos.company.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var securityApp = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Code = ApplicationCodes.Security,
            Name = "Sistema de Seguridad",
            Icon = "shield",
            BaseUrl = "https://security.company.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Applications.AddRangeAsync(auditoriaApp, memosApp, securityApp);

        // Create roles
        var superAdminRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = securityApp.ApplicationId,
            Code = "SUPERADMIN",
            Name = "Super Administrador",
            Description = "Acceso total al sistema de seguridad",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var auditorAdminRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = auditoriaApp.ApplicationId,
            Code = "AUDITOR_ADMIN",
            Name = "Administrador de Auditoría",
            Description = "Administrador del sistema de auditoría",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memoUserRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "MEMO_USER",
            Name = "Usuario de Memos",
            Description = "Usuario básico del sistema de memos",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Roles específicos para Sistema de gestión y trazabilidad de documentos
        var memoAdministradorRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "R001",
            Name = "Administrador",
            Description = "Acceso de tienda",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memoSupervisorRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "R002",
            Name = "Supervisor",
            Description = "Gestión de memos y empleados de su tienda",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memoEmpleadoRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "R003",
            Name = "Empleado",
            Description = "Acceso limitado para firmar memos",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memoRRHHRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "R004",
            Name = "RRHH",
            Description = "Recursos Humanos - gestión de empleados y reportes y memorandums",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memoSysAdminRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "R005",
            Name = "SYSADMIN",
            Description = "Administrador del sistema",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memoJefaturaRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "R006",
            Name = "Jefatura",
            Description = "Jefatura - Solicita memorandums",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Roles.AddRangeAsync(
            superAdminRole, 
            auditorAdminRole, 
            memoUserRole,
            memoAdministradorRole,
            memoSupervisorRole,
            memoEmpleadoRole,
            memoRRHHRole,
            memoSysAdminRole,
            memoJefaturaRole);

        // Create modules
        var auditoriaExpedientesModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            Code = "M0001",
            ApplicationId = auditoriaApp.ApplicationId,
            Name = "EXPEDIENTES",
            Description = "Módulo de gestión de expedientes",
            Icon = "fa-folder",
            Order = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Modules for Sistema de gestión y trazabilidad de documentos
        var memosInicioModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = "M0001",
            Name = "Inicio",
            Description = "Dashboard principal del sistema",
            Icon = "home",
            Order = 1,
            CreatedAt = DateTime.UtcNow
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
            CreatedAt = DateTime.UtcNow
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
            CreatedAt = DateTime.UtcNow
        };

        await context.Modules.AddRangeAsync(
            auditoriaExpedientesModule, 
            memosInicioModule, 
            memosTrazabilidadModule, 
            memosAdministracionModule);

        // Create options for Auditoria
        var auditoriaExpedientesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = auditoriaExpedientesModule.ModuleId,
            Code = "EXPEDIENTES",
            Name = "EXPEDIENTES",
            Icon = "fa-search",
            Route = "/api/expedientes",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var auditoriaExpedientesPostOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = auditoriaExpedientesModule.ModuleId,
            Code = "EXPEDIENTES",
            Name = "EXPEDIENTES",
            Icon = "fa-plus",
            Route = "/api/expedientes",
            HttpMethod = "POST",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create options for Memos - Inicio Module
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

        // Create options for Memos - Trazabilidad Module
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

        // Create options for Memos - Administración Module
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
            auditoriaExpedientesOption,
            auditoriaExpedientesPostOption,
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

        // Create permissions
        var auditorPermissionRead = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = auditorAdminRole.RoleId,
            OptionId = auditoriaExpedientesOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var auditorPermissionCreate = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = auditorAdminRole.RoleId,
            OptionId = auditoriaExpedientesPostOption.OptionId,
            ActionCode = ActionCodes.Create,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Permisos para SYSADMIN (acceso completo)
        var sysAdminPermissionDashboard = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosDashboardOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionMemorandum = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosMemorandumOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionBandeja = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosBandejaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionReportes = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosReportesOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionUsuarios = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosUsuariosOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionPerfiles = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosPerfilesOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionEmpresa = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosEmpresaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionTienda = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosTiendaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionEmpleados = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosEmpleadosOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionSancion = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosSancionOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionTemplate = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosTemplateOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var sysAdminPermissionSecuencia = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSysAdminRole.RoleId,
            OptionId = memosSecuenciaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Permiso Bandeja para Administrador
        var adminPermissionBandeja = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoAdministradorRole.RoleId,
            OptionId = memosBandejaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Permisos para RRHH
        var rrhhPermissionDashboard = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosDashboardOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var rrhhPermissionMemorandum = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosMemorandumOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var rrhhPermissionReportes = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosReportesOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var rrhhPermissionEmpleados = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosEmpleadosOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Permisos adicionales para RRHH (mismos que SYSADMIN excepto Template y Secuencia)
        var rrhhPermissionBandeja = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosBandejaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var rrhhPermissionUsuarios = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosUsuariosOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var rrhhPermissionPerfiles = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosPerfilesOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var rrhhPermissionEmpresa = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosEmpresaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var rrhhPermissionTienda = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosTiendaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var rrhhPermissionSancion = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoRRHHRole.RoleId,
            OptionId = memosSancionOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Permiso Bandeja para Supervisor
        var supervisorPermissionBandeja = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoSupervisorRole.RoleId,
            OptionId = memosBandejaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Permisos para Jefatura
        var jefaturaPermissionBandeja = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoJefaturaRole.RoleId,
            OptionId = memosBandejaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Permisos para Empleado (acceso limitado)
        var empleadoPermissionBandeja = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoEmpleadoRole.RoleId,
            OptionId = memosBandejaOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Permissions.AddRangeAsync(
            auditorPermissionRead,
            auditorPermissionCreate,
            // SYSADMIN - Acceso completo
            sysAdminPermissionDashboard,
            sysAdminPermissionMemorandum,
            sysAdminPermissionBandeja,
            sysAdminPermissionReportes,
            sysAdminPermissionUsuarios,
            sysAdminPermissionPerfiles,
            sysAdminPermissionEmpresa,
            sysAdminPermissionTienda,
            sysAdminPermissionEmpleados,
            sysAdminPermissionSancion,
            sysAdminPermissionTemplate,
            sysAdminPermissionSecuencia,
            // Administrador
            adminPermissionBandeja,
            // RRHH - Mismos permisos que SYSADMIN excepto Template y Secuencia
            rrhhPermissionDashboard,
            rrhhPermissionMemorandum,
            rrhhPermissionBandeja,
            rrhhPermissionReportes,
            rrhhPermissionUsuarios,
            rrhhPermissionPerfiles,
            rrhhPermissionEmpresa,
            rrhhPermissionTienda,
            rrhhPermissionEmpleados,
            rrhhPermissionSancion,
            // Supervisor
            supervisorPermissionBandeja,
            // Jefatura
            jefaturaPermissionBandeja,
            // Empleado
            empleadoPermissionBandeja);

        // Create admin user
        var adminUser = new User
        {
            UserId = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = passwordService.HashPassword("admin123"),
            Status = UserStatus.Active,
            EmployeeId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null,
            PasswordResetToken = null,
            PasswordResetTokenExpires = null
        };

        await context.Users.AddAsync(adminUser);

        // Assign applications to user
        var userAuditoriaApp = new UserApplication
        {
            UserId = adminUser.UserId,
            ApplicationId = auditoriaApp.ApplicationId,
            IsActive = true,
            AssignedAt = DateTime.UtcNow
        };

        var userMemosApp = new UserApplication
        {
            UserId = adminUser.UserId,
            ApplicationId = memosApp.ApplicationId,
            IsActive = true,
            AssignedAt = DateTime.UtcNow
        };

        var userSecurityApp = new UserApplication
        {
            UserId = adminUser.UserId,
            ApplicationId = securityApp.ApplicationId,
            IsActive = true,
            AssignedAt = DateTime.UtcNow
        };

        await context.UserApplications.AddRangeAsync(
            userAuditoriaApp,
            userMemosApp,
            userSecurityApp);

        // Assign roles to user
        var userSuperAdminRole = new UserRole
        {
            UserId = adminUser.UserId,
            RoleId = superAdminRole.RoleId,
            AssignedAt = DateTime.UtcNow
        };

        var userAuditorRole = new UserRole
        {
            UserId = adminUser.UserId,
            RoleId = auditorAdminRole.RoleId,
            AssignedAt = DateTime.UtcNow
        };

        var userMemoRole = new UserRole
        {
            UserId = adminUser.UserId,
            RoleId = memoSysAdminRole.RoleId, // Asignar rol SYSADMIN
            AssignedAt = DateTime.UtcNow
        };

        await context.UserRoles.AddRangeAsync(
            userSuperAdminRole,
            userAuditorRole,
            userMemoRole);

        await context.SaveChangesAsync();
    }
}