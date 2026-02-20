using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Constants;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Data.Seeds.Clients.SIZE;

public static class SizeApplicationSeedData
{
    public static async Task SeedAsync(SecurityDbContext context)
    {
        // Skip if SIZE application already exists
        if (await context.Applications.AnyAsync(a => a.Code == ApplicationCodes.Size))
            return;

        var sizeApp = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Code = ApplicationCodes.Size,
            Name = "Mi Talla",
            Icon = "straighten",
            BaseUrl = "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Applications.Add(sizeApp);
        await context.SaveChangesAsync();
    }
}
