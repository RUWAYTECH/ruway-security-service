using SecurityMicroservice.Domain.Entities;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;

namespace SecurityMicroservice.Infrastructure.Repositories;

public class UserApplicationRepository : EFRepository<UserApplication>, IUserApplicationRepository
{
    public UserApplicationRepository(SecurityDbContext context) : base(context)
    {
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

    public async Task<bool> ExistsAsync(Guid userId, Guid applicationId)
    {
        return await AnyAsync(ua => ua.UserId == userId && ua.ApplicationId == applicationId);
    }
}