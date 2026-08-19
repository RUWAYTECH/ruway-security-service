using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;

namespace SecurityMicroservice.Infrastructure.Repositories;

public class OptionRepository : EFRepository<Option>, IOptionRepository
{
    public OptionRepository(SecurityDbContext context) : base(context)
    {
    }

    public async Task<List<Option>> GetByModuleIdAsync(Guid moduleId)
    {
        return await Db.Options
            .Include(o => o.Module)
                .ThenInclude(m => m.Application)
            .Where(o => o.ModuleId == moduleId)
            .OrderBy(o => o.Order)
            .ThenBy(o => o.Name)
            .ToListAsync();
    }

    public async Task<List<Option>> GetByApplicationCodeAsync(string applicationCode)
    {
        return await Db.Options
            .Include(o => o.Module)
                .ThenInclude(m => m.Application)
            .Where(o => o.Module.Application.Code == applicationCode)
            .OrderBy(o => o.Module.Order)
            .ThenBy(o => o.Order)
            .ThenBy(o => o.Name)
            .ToListAsync();
    }

    public async Task<int> GetMaxOrderByModuleIdAsync(Guid moduleId)
    {
        return await Db.Options
            .Where(o => o.ModuleId == moduleId)
            .MaxAsync(o => (int?)o.Order) ?? 0;
    }

    public async Task<Option?> GetByCodeAsync(string code)
    {
        return await Db.Options
            .Include(o => o.Module)
                .ThenInclude(m => m.Application)
            .FirstOrDefaultAsync(o => o.Code == code);
    }
}