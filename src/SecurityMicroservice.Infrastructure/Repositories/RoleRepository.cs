using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;

namespace SecurityMicroservice.Infrastructure.Repositories;

public class RoleRepository : EFRepository<Role>, IRoleRepository
{
    public RoleRepository(SecurityDbContext context) : base(context)
    {
    }
}