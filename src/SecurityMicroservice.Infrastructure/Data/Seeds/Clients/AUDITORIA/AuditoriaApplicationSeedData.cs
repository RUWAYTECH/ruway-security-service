using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.AUDITORIA;

public static class AuditoriaApplicationSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        // Skip if AUDITORIA application already exists
        if (await context.Applications.AnyAsync(a => a.Code == ApplicationCodes.Auditoria))
            return;

        var auditoriaApp = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Code = ApplicationCodes.Auditoria,
            Name = "Sistema de Auditoría", 
            Icon = "store",
            BaseUrl = "https://auditoria.company.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Applications.Add(auditoriaApp);
        await context.SaveChangesAsync();
    }
}