using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.SIZE;

public static class SizeRolesSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        var sizeApp = await context.Applications.FirstOrDefaultAsync(a => a.Code == ApplicationCodes.Size);
        if (sizeApp == null) return;

        var sizeRoles = new[]
        {
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = sizeApp.ApplicationId,
                Code = "APPADMIN",
                Name = "APPADMIN - Administrador de aplicación",
                Description = "Usuario Administrador de aplicación",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = sizeApp.ApplicationId,
                Code = "GG",
                Name = "Gerente General",
                Description = "Gerente General",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = sizeApp.ApplicationId,
                Code = "JC",
                Name = "Jefe de Calidad",
                Description = "Jefe de Calidad",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = sizeApp.ApplicationId,
                Code = "AC",
                Name = "Auditor de Calidad",
                Description = "Auditor de Calidad",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = sizeApp.ApplicationId,
                Code = "AV",
                Name = "Administrador de Ventas",
                Description = "Administrador de Ventas",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = sizeApp.ApplicationId,
                Code = "SASTRE",
                Name = "Sastres",
                Description = "Sastres",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                RoleId = Guid.NewGuid(),
                ApplicationId = sizeApp.ApplicationId,
                Code = "PCB01",
                Name = "Beneficiario",
                Description = "Beneficiario",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var role in sizeRoles)
        {
            if (!await context.Roles.AnyAsync(r => r.Code == role.Code && r.ApplicationId == role.ApplicationId))
            {
                context.Roles.Add(role);
            }
        }
        await context.SaveChangesAsync();
    }
}
