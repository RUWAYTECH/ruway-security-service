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
            BaseUrl = "https://auditoria.company.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var memosApp = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Code = ApplicationCodes.Memos,
            Name = "Sistema de Memos",
            BaseUrl = "https://memos.company.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var securityApp = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Code = ApplicationCodes.Security,
            Name = "Sistema de Seguridad",
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

        await context.Roles.AddRangeAsync(superAdminRole, auditorAdminRole, memoUserRole);

        // Create modules
        var auditoriaExpedientesModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            Code = ApplicationCodes.Auditoria,
            ApplicationId = auditoriaApp.ApplicationId,
            Name = "EXPEDIENTES",
            Description = "Módulo de gestión de expedientes",
            Icon = "fa-folder",
            Order = 1,
            CreatedAt = DateTime.UtcNow
        };

        var memosDocumentosModule = new Module
        {
            ModuleId = Guid.NewGuid(),
            ApplicationId = memosApp.ApplicationId,
            Code = ApplicationCodes.Memos,
            Name = "DOCUMENTOS",
            Description = "Módulo de gestión de documentos",
            Icon = "fa-file",
            Order = 1,
            CreatedAt = DateTime.UtcNow
        };

        await context.Modules.AddRangeAsync(auditoriaExpedientesModule, memosDocumentosModule);

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

        // Create options for Memos
        var memosDocumentosOption = new Option
        {
            OptionId = Guid.NewGuid(),
            ModuleId = memosDocumentosModule.ModuleId,
            Code = "DOCUMENTOS",
            Name = "DOCUMENTOS",
            Icon = "fa-file-text",
            Route = "/api/documentos",
            HttpMethod = "GET",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Options.AddRangeAsync(
            auditoriaExpedientesOption,
            auditoriaExpedientesPostOption,
            memosDocumentosOption);

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

        var memoPermissionRead = new Permission
        {
            PermissionId = Guid.NewGuid(),
            RoleId = memoUserRole.RoleId,
            OptionId = memosDocumentosOption.OptionId,
            ActionCode = ActionCodes.Read,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Permissions.AddRangeAsync(
            auditorPermissionRead,
            auditorPermissionCreate,
            memoPermissionRead);

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
            RoleId = memoUserRole.RoleId,
            AssignedAt = DateTime.UtcNow
        };

        await context.UserRoles.AddRangeAsync(
            userSuperAdminRole,
            userAuditorRole,
            userMemoRole);

        await context.SaveChangesAsync();
    }
}