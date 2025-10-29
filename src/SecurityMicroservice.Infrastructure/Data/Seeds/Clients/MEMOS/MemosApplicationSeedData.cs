using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.MEMOS;

public static class MemosApplicationSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        // Skip if MEMOS application already exists
        if (await context.Applications.AnyAsync(a => a.Code == ApplicationCodes.Memos))
            return;

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

        context.Applications.Add(memosApp);
        await context.SaveChangesAsync();
    }
}