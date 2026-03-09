using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Services;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Core;

/// <summary>
/// Seed data for the core Security System
/// This contains only the essential security framework data that is reusable across different clients
/// </summary>
public static class CoreSecuritySeedData
{
    public static async Task InitializeAsync(SecurityDbContext context, IPasswordService passwordService)
    {
        // Check if core security data already exists
        if (await context.Applications.AnyAsync(a => a.Code == ApplicationCodes.Security))
        {
            Console.WriteLine("ℹ️ Core Security System data already exists.");
            return;
        }

        Console.WriteLine("🚀 Initializing Core Security System...");

        // === CORE SECURITY APPLICATION ===
        var securityApp = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Code = ApplicationCodes.Security,
            Name = "Sistema de Seguridad",
            Icon = "security",
            BaseUrl = "https://security.company.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Applications.AddAsync(securityApp);

        // === CORE SECURITY ROLES ===
        var superAdminRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = securityApp.ApplicationId,
            Code = "SUPERADMIN",
            Name = "Super Administrador",
            Description = "Acceso total al sistema de seguridad",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        var securityAppAdminRole = new Role
        {
            RoleId = Guid.NewGuid(),
            ApplicationId = securityApp.ApplicationId,
            Code = "APPADMIN",
            Name = "Administrador de aplicación",
            Description = "Administrador del sistema de seguridad con permisos completos",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Roles.AddRangeAsync(superAdminRole, securityAppAdminRole);

        // === CORE SECURITY MODULES ===
        var securityAdministracionModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = securityApp.ApplicationId,
            Code = "SEC_M001",
            Name = "Administración",
            Description = "Módulo de administración del sistema de seguridad",
            Icon = "admin_panel_settings",
            Order = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await context.Modules.AddAsync(securityAdministracionModule);

        // === CORE SECURITY OPTIONS ===
        var securityAplicacionesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = securityAdministracionModule.ModuleId,
            Code = "SEC_OP001",
            Name = "Aplicaciones",
            Icon = "apps",
            Route = "/secure/applications",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var securityUsuariosOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = securityAdministracionModule.ModuleId,
            Code = "SEC_OP002",
            Name = "Usuarios",
            Icon = "group",
            Route = "/secure/users",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var securityRolesOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = securityAdministracionModule.ModuleId,
            Code = "SEC_OP003",
            Name = "Roles",
            Icon = "assignment_ind",
            Route = "/secure/roles",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var securityModulosOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = securityAdministracionModule.ModuleId,
            Code = "SEC_OP004",
            Name = "Módulos",
            Icon = "view_module",
            Route = "/secure/modules",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var securityUsuariosAplicacionOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = securityAdministracionModule.ModuleId,
            Code = "SEC_OP005",
            Name = "Usuarios Aplicación",
            Icon = "manage_accounts",
            Route = "/secure/user-applications",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Options.AddRangeAsync(
            securityAplicacionesOption,
            securityUsuariosOption,
            securityRolesOption,
            securityModulosOption,
            securityUsuariosAplicacionOption);

        // === CORE SECURITY PERMISSIONS (Full CRUD for APP_ADMIN) ===
        var permissions = new List<Permission>();

        // Helper method to create multiple permissions
        void AddPermissions(Guid optionId, params string[] actions)
        {
            foreach (var action in actions)
            {
                permissions.Add(new Permission
                {
                    PermissionId = Guid.NewGuid(),
                    RoleId = securityAppAdminRole.RoleId,
                    OptionId = optionId,
                    ActionCode = action,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Full CRUD permissions for all security options
        AddPermissions(securityAplicacionesOption.OptionId, 
            ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);
        AddPermissions(securityUsuariosOption.OptionId, 
            ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);
        AddPermissions(securityRolesOption.OptionId, 
            ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);
        AddPermissions(securityModulosOption.OptionId, 
            ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);
        AddPermissions(securityUsuariosAplicacionOption.OptionId, 
            ActionCodes.Create, ActionCodes.Read, ActionCodes.Update, ActionCodes.Delete);

        await context.Permissions.AddRangeAsync(permissions);

        // === DEFAULT ADMIN USER ===
        var adminUser = new User
        {
            UserId = Guid.Parse("EEEEEEEE-1111-1111-1111-111111111111"),
            LastName = "Admin",
            FirstName = "Super",
            UserName = "admin",
            PasswordHash = passwordService.HashPassword("admin123"),
            Status = UserStatus.Active,
            IsExternal = false,
            EmployeeId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null,
            PasswordResetToken = null,
            PasswordResetTokenExpires = null
        };

        await context.Users.AddAsync(adminUser);

        // Assign Security application to admin user
        var userSecurityApp = new UserApplication
        {
            UserId = adminUser.UserId,
            ApplicationId = securityApp.ApplicationId,
            IsActive = true,
            AssignedAt = DateTime.UtcNow
        };

        await context.UserApplications.AddAsync(userSecurityApp);

        // Assign roles to admin user
        var userSuperAdminRole = new UserRole
        {
            UserId = adminUser.UserId,
            RoleId = superAdminRole.RoleId,
            AssignedAt = DateTime.UtcNow
        };

        var userSecurityAppAdminRole = new UserRole
        {
            UserId = adminUser.UserId,
            RoleId = securityAppAdminRole.RoleId,
            AssignedAt = DateTime.UtcNow
        };

        await context.UserRoles.AddRangeAsync(userSuperAdminRole, userSecurityAppAdminRole);

        await context.SaveChangesAsync();

        Console.WriteLine("✅ Core Security System initialized successfully:");
        Console.WriteLine("   - 1 Application (Security System)");
        Console.WriteLine("   - 2 Core Roles (SUPERADMIN, APP_ADMIN)");
        Console.WriteLine("   - 1 Module (Administration)");
        Console.WriteLine("   - 5 Options (Applications, Users, Roles, Modules, User-Applications)");
        Console.WriteLine($"   - {permissions.Count} Permissions assigned");
        Console.WriteLine("   - 1 Default Admin User created");
    }
}