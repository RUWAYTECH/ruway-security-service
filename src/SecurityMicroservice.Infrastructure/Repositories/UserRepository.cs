using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;

namespace SecurityMicroservice.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByUsernameAsync(string username);
    Task<List<User>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid userId);
    Task<List<User>> GetByApplicationIdAsync(Guid applicationId);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
    Task<List<Permission>> GetUserPermissionEntitiesAsync(Guid userId);
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<List<string>> GetUserApplicationScopesAsync(Guid userId);
    Task<User?> GetByTokenAsync(string token);
}

public class UserRepository : IUserRepository
{
    private readonly SecurityDbContext _context;

    public UserRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.Application)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                    .ThenInclude(p => p.Option)
                        .ThenInclude(o => o.Module)
                            .ThenInclude(m => m.Application)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.Application)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                    .ThenInclude(p => p.Option)
                        .ThenInclude(o => o.Module)
                            .ThenInclude(m => m.Application)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<User>> GetByApplicationIdAsync(Guid applicationId)
    {
        return await _context.Users
            .Where(u => u.UserApplications.Any(ua => ua.ApplicationId == applicationId && ua.IsActive))
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .Include(up => up.Permission)
                .ThenInclude(p => p.Option)
                    .ThenInclude(o => o.Module)
                        .ThenInclude(m => m.Application)
            .Where(up => up.Permission.IsActive && up.Permission.Option.IsActive && up.Permission.Option.Module.Application.IsActive)
            .Select(up => $"{up.Permission.Option.Module.Application.Code}:{up.Permission.Option.Name}:{up.Permission.ActionCode}")
            .ToListAsync();

        var rolePermissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.Permissions)
                    .ThenInclude(p => p.Option)
                        .ThenInclude(o => o.Module)
                            .ThenInclude(m => m.Application)
            .SelectMany(ur => ur.Role.Permissions)
            .Where(p => p.IsActive && p.Option.IsActive && p.Option.Module.Application.IsActive)
            .Select(p => $"{p.Option.Module.Application.Code}:{p.Option.Name}:{p.ActionCode}")
            .ToListAsync();

        return permissions.Concat(rolePermissions).Distinct().ToList();
    }

    public async Task<List<Permission>> GetUserPermissionEntitiesAsync(Guid userId)
    {
        // Obtener permisos directos del usuario
        var directPermissions = await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .Include(up => up.Permission)
                .ThenInclude(p => p.Option)
                    .ThenInclude(o => o.Module)
                        .ThenInclude(m => m.Application)
            .Where(up => up.Permission.IsActive && up.Permission.Option.IsActive && up.Permission.Option.Module.Application.IsActive)
            .Select(up => up.Permission)
            .ToListAsync();

        // Obtener permisos a través de roles
        var rolePermissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.Permissions)
                    .ThenInclude(p => p.Option)
                        .ThenInclude(o => o.Module)
                            .ThenInclude(m => m.Application)
            .SelectMany(ur => ur.Role.Permissions)
            .Where(p => p.IsActive && p.Option.IsActive && p.Option.Module.Application.IsActive)
            .ToListAsync();

        // Combinar y eliminar duplicados
        var allPermissions = directPermissions.Concat(rolePermissions)
            .GroupBy(p => new { p.PermissionId })
            .Select(g => g.First())
            .ToList();

        return allPermissions;
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.Application)
            .Where(ur => ur.Role.IsActive && ur.Role.Application.IsActive)
            .Select(ur => $"{ur.Role.Application.Code}_{ur.Role.Code}")
            .ToListAsync();
    }

    public async Task<List<string>> GetUserApplicationScopesAsync(Guid userId)
    {
        return await _context.UserApplications
            .Where(ua => ua.UserId == userId && ua.IsActive)
            .Include(ua => ua.Application)
            .Where(ua => ua.Application.IsActive)
            .Select(ua => ua.Application.Code.ToLower())
            .ToListAsync();
    }

    public async Task<User?> GetByTokenAsync(string token)
    {
        User? user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PasswordResetToken == token
                        && u.PasswordResetTokenExpires > DateTime.UtcNow
                        && u.Status == UserStatus.Active);
        return user;
    }
}