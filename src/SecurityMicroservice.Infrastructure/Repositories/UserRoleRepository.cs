using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;
using System.Linq.Expressions;

namespace SecurityMicroservice.Infrastructure.Repositories;

public interface IUserRoleRepository : IRepository<UserRole>
{
    Task<UserRole?> GetByIdAsync(Guid userId, Guid roleId);
    Task<List<UserRole>> GetByUserIdAsync(Guid userId);
    Task<List<UserRole>> GetByRoleIdAsync(Guid roleId);
    Task<UserRole> CreateAsync(UserRole userRole);
    Task<UserRole> UpdateAsync(UserRole userRole);
    Task DeleteAsync(Guid userId, Guid roleId);
    Task<bool> ExistsAsync(Guid userId, Guid roleId);
}

public class UserRoleRepository : EFRepository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(SecurityDbContext context) : base(context)
    {
    }

    public async Task<UserRole?> GetByIdAsync(Guid userId, Guid roleId)
    {
        return await GetFirstOrDefaultAsync(
            ur => ur.UserId == userId && ur.RoleId == roleId,
            q => q.OrderBy(x => x.AssignedAt),
            default,
            ur => ur.User, ur => ur.Role);
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

    public async Task<UserRole> CreateAsync(UserRole userRole)
    {
        Insert(userRole);
        await Db.SaveChangesAsync();
        return await GetByIdAsync(userRole.UserId, userRole.RoleId) ?? userRole;
    }

    public async Task<UserRole> UpdateAsync(UserRole userRole)
    {
        Update(userRole);
        await Db.SaveChangesAsync();
        return await GetByIdAsync(userRole.UserId, userRole.RoleId) ?? userRole;
    }

    public async Task DeleteAsync(Guid userId, Guid roleId)
    {
        var userRole = await GetByIdAsync(userId, roleId);
        if (userRole != null)
        {
            Delete(userRole);
            await Db.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid roleId)
    {
        return await AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }
}