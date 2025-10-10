using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;

namespace SecurityMicroservice.Infrastructure.Repositories;

public class ModuleRepository : EFRepository<Module>, IModuleRepository
{
    public ModuleRepository(SecurityDbContext context) : base(context)
    {
    }
}