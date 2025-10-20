using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
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
            ua => ua.UserId == userId && ua.User.Status == UserStatus.Active,
            q => q.OrderBy(x => x.AssignedAt),
            ua => ua.User, ua => ua.Application);
    }

    public async Task<List<UserApplication>> GetByApplicationIdAsync(Guid applicationId)
    {
        return await GetAsync(
            ua => ua.ApplicationId == applicationId && ua.IsActive && ua.User.Status == UserStatus.Active,
            q => q.OrderBy(x => x.AssignedAt),
            ua => ua.User.UserRoles.Select(a => a.Role), ua => ua.Application);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid applicationId)
    {
        return await AnyAsync(ua => ua.UserId == userId && ua.User.Status == UserStatus.Active && ua.ApplicationId == applicationId);
    }

    public async Task<(List<UserApplication> Items, int TotalRows)> GetUserApplicationPagedAsync(
           Expression<Func<UserApplication, bool>> filter = null,
           Func<IQueryable<UserApplication>, IOrderedQueryable<UserApplication>> orderBy = null,
           int pageNumber = 0,
           int pageSize = 0
    )
    {
        IQueryable<UserApplication> query = Db.UserApplications
                .Include(u => u.User)
                .Include(u => u.Application)
                .Include(u => u.User.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ThenInclude(ap => ap.Application);


        if (filter != null)
            query = query.Where(filter);

        var totalRows = await query.CountAsync();

        if (orderBy != null)
            query = orderBy(query);

        if (pageNumber > 0 && pageSize > 0)
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var items = await query.ToListAsync();

        return (items, totalRows);
    }

}