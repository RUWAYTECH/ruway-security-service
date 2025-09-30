using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;

namespace SecurityMicroservice.Infrastructure.Repositories;

public class UserRoleRepository : EFRepository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(SecurityDbContext context) : base(context)
    {
    }

    public async Task<List<UserRole>> GetByUserIdAsync(Guid userId)
    {
        return await GetAsync(
            ur => ur.UserId == userId,
            q => q.OrderBy(x => x.AssignedAt),
            ur => ur.User, 
            ur => ur.Role);
    }

    public async Task<List<UserRole>> GetByRoleIdAsync(Guid roleId)
    {
        return await GetAsync(
            ur => ur.RoleId == roleId,
            q => q.OrderBy(x => x.AssignedAt),
            ur => ur.User, 
            ur => ur.Role);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid roleId)
    {
        return await AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }
}