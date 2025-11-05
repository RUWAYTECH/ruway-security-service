using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.MEMOS;

public static class MemosRolesSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        var memosApp = await context.Applications.FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Memos);
        if (memosApp == null) return;

        var memosRoles = new[]
        {
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = memosApp.ApplicationId,
                Code = "APPADMIN",
                Name = "Administrador de aplicación",
                Description = "Usuario Administrador de aplicación",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = memosApp.ApplicationId,
                Code = "R001",
                Name = "Administrador",
                Description = "Acceso de tienda",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = memosApp.ApplicationId,
                Code = "R002",
                Name = "Supervisor",
                Description = "Gestión de memos y empleados de su tienda",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = memosApp.ApplicationId,
                Code = "R003",
                Name = "Empleado",
                Description = "Acceso limitado para firmar memos",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = memosApp.ApplicationId,
                Code = "R004",
                Name = "RRHH",
                Description = "Recursos Humanos - gestión de empleados y reportes y memorandums",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = memosApp.ApplicationId,
                Code = "R006",
                Name = "Jefatura",
                Description = "Jefatura - Solicita memorandums",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var role in memosRoles)
        {
            if (!await context.Roles.AnyAsync(r => r.Code == role.Code && r.ApplicationId == role.ApplicationId))
            {
                context.Roles.Add(role);
            }
        }

        await context.SaveChangesAsync();
    }
}