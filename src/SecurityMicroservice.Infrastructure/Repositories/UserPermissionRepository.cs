using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;
using System.Linq.Expressions;

namespace SecurityMicroservice.Infrastructure.Repositories;

public interface IUserPermissionRepository : IRepository<UserPermission>
{
    Task<UserPermission?> GetByIdAsync(Guid userId, Guid permissionId);
    Task<List<UserPermission>> GetByUserIdAsync(Guid userId);
    Task<List<UserPermission>> GetByPermissionIdAsync(Guid permissionId);
    Task<UserPermission> CreateAsync(UserPermission userPermission);
    Task<UserPermission> UpdateAsync(UserPermission userPermission);
    Task DeleteAsync(Guid userId, Guid permissionId);
    Task<bool> ExistsAsync(Guid userId, Guid permissionId);
}

public class UserPermissionRepository : EFRepository<UserPermission>, IUserPermissionRepository
{
    public UserPermissionRepository(SecurityDbContext context) : base(context)
    {
    }

    public async Task<UserPermission?> GetByIdAsync(Guid userId, Guid permissionId)
    {
        return await GetFirstOrDefaultAsync(
            up => up.UserId == userId && up.PermissionId == permissionId,
            q => q.OrderBy(x => x.GrantedAt),
            default,
            up => up.User,
            up => up.Permission,
            up => up.GrantedByUser);
    }

    public async Task<List<UserPermission>> GetByUserIdAsync(Guid userId)
    {
        return await GetAsync(
            up => up.UserId == userId,
            q => q.OrderBy(x => x.GrantedAt),
            up => up.User,
            up => up.Permission,
            up => up.GrantedByUser);
    }

    public async Task<List<UserPermission>> GetByPermissionIdAsync(Guid permissionId)
    {
        return await GetAsync(
            up => up.PermissionId == permissionId,
            q => q.OrderBy(x => x.GrantedAt),
            up => up.User,
            up => up.Permission,
            up => up.GrantedByUser);
    }

    public async Task<UserPermission> CreateAsync(UserPermission userPermission)
    {
        Insert(userPermission);
        await Db.SaveChangesAsync();
        return await GetByIdAsync(userPermission.UserId, userPermission.PermissionId) ?? userPermission;
    }

    public async Task<UserPermission> UpdateAsync(UserPermission userPermission)
    {
        Update(userPermission);
        await Db.SaveChangesAsync();
        return await GetByIdAsync(userPermission.UserId, userPermission.PermissionId) ?? userPermission;
    }

    public async Task DeleteAsync(Guid userId, Guid permissionId)
    {
        var userPermission = await GetByIdAsync(userId, permissionId);
        if (userPermission != null)
        {
            Delete(userPermission);
            await Db.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid permissionId)
    {
        return await AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId);
    }
}