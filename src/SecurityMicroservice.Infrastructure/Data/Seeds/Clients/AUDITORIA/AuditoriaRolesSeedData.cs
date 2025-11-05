using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.AUDITORIA;

public static class AuditoriaRolesSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        var auditoriaApp = await context.Applications.FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Auditoria);
        if (auditoriaApp == null) return;

        var auditoriaRoles = new[]
        {
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = auditoriaApp.ApplicationId,
                Code = "APPADMIN",
                Name = "Administrador de aplicación",
                Description = "Usuario Administrador de aplicación",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = auditoriaApp.ApplicationId,
                Code = "A001",
                Name = "Administrador",
                Description = "Administrador de auditoría",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = auditoriaApp.ApplicationId,
                Code = "A002",
                Name = "Asistente",
                Description = "Asistente de auditoría",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = auditoriaApp.ApplicationId,
                Code = "A003",
                Name = "Jefe de operaciones",
                Description = "Jefe de Operaciones regional",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = auditoriaApp.ApplicationId,
                Code = "A004",
                Name = "Volante",
                Description = "Auditor volante",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = auditoriaApp.ApplicationId,
                Code = "A005",
                Name = "Auditor",
                Description = "Auditor",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = auditoriaApp.ApplicationId,
                Code = "A006",
                Name = "JOB/Supervisor",
                Description = "Supervisor de auditoría",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var role in auditoriaRoles)
        {
            if (!await context.Roles.AnyAsync(r => r.Code == role.Code && r.ApplicationId == role.ApplicationId))
            {
                context.Roles.Add(role);
            }
        }

        await context.SaveChangesAsync();
    }
}