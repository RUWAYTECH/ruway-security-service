using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;

namespace SecurityMicroservice.Infrastructure.Repositories;

public class PermissionRepository : EFRepository<Permission>, IPermissionRepository
{
    private readonly SecurityDbContext _context;

    public PermissionRepository(SecurityDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid permissionId)
    {
        return await _context.Permissions
            .Include(u => u.Role)
            .Include(u => u.Option)
            .FirstOrDefaultAsync(u => u.PermissionId == permissionId);
    }

    public async Task<List<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            .Include(u => u.Role)
            .Include(u => u.Option)
            .ToListAsync();
    }

    public async Task<Permission> CreateAsync(Permission permission)
    {
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task<Permission> UpdateAsync(Permission permission)
    {
        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task DeleteAsync(Guid permissionId)
    {
        var user = await _context.Permissions.FindAsync(permissionId);
        if (user != null)
        {
            _context.Permissions.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
