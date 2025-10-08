using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;

namespace SecurityMicroservice.Infrastructure.Repositories;

public class ApplicationRepository : EFRepository<Application>, IApplicationRepository
{
    public ApplicationRepository(SecurityDbContext context) : base(context)
    {
    }
}