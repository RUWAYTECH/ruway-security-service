using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;
using System.Linq.Expressions;

namespace SecurityMicroservice.Infrastructure.Repositories;

public interface IUserApplicationRepository : IRepository<UserApplication>
{
    Task<UserApplication?> GetByIdAsync(Guid userId, Guid applicationId);
    Task<List<UserApplication>> GetByUserIdAsync(Guid userId);
    Task<List<UserApplication>> GetByApplicationIdAsync(Guid applicationId);
    Task<UserApplication> CreateAsync(UserApplication userApplication);
    Task<UserApplication> UpdateAsync(UserApplication userApplication);
    Task DeleteAsync(Guid userId, Guid applicationId);
    Task<bool> ExistsAsync(Guid userId, Guid applicationId);
}

public class UserApplicationRepository : EFRepository<UserApplication>, IUserApplicationRepository
{
    public UserApplicationRepository(SecurityDbContext context) : base(context)
    {
    }

    public async Task<UserApplication?> GetByIdAsync(Guid userId, Guid applicationId)
    {
        return await GetFirstOrDefaultAsync(
            ua => ua.UserId == userId && ua.ApplicationId == applicationId,
            q => q.OrderBy(x => x.AssignedAt),
            default,
            ua => ua.User, ua => ua.Application);
    }

    public async Task<List<UserApplication>> GetByUserIdAsync(Guid userId)
    {
        return await GetAsync(
            ua => ua.UserId == userId,
            q => q.OrderBy(x => x.AssignedAt),
            ua => ua.User, ua => ua.Application);
    }

    public async Task<List<UserApplication>> GetByApplicationIdAsync(Guid applicationId)
    {
        return await GetAsync(
            ua => ua.ApplicationId == applicationId,
            q => q.OrderBy(x => x.AssignedAt),
            ua => ua.User, ua => ua.Application);
    }

    public async Task<UserApplication> CreateAsync(UserApplication userApplication)
    {
        Insert(userApplication);
        await Db.SaveChangesAsync();
        return await GetByIdAsync(userApplication.UserId, userApplication.ApplicationId) ?? userApplication;
    }

    public async Task<UserApplication> UpdateAsync(UserApplication userApplication)
    {
        Update(userApplication);
        await Db.SaveChangesAsync();
        return await GetByIdAsync(userApplication.UserId, userApplication.ApplicationId) ?? userApplication;
    }

    public async Task DeleteAsync(Guid userId, Guid applicationId)
    {
        var userApplication = await GetByIdAsync(userId, applicationId);
        if (userApplication != null)
        {
            Delete(userApplication);
            await Db.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid applicationId)
    {
        return await AnyAsync(ua => ua.UserId == userId && ua.ApplicationId == applicationId);
    }
}