using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;

namespace SecurityMicroservice.Infrastructure.Repositories;

public class PermissionRepository : EFRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(SecurityDbContext context) : base(context)
    {
    }
}
