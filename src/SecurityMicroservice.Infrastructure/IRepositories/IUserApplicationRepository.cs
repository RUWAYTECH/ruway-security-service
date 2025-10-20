using System.Linq.Expressions;
using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.IRepositories
{
    public interface IUserApplicationRepository : IRepository<UserApplication>
    {
        Task<List<UserApplication>> GetByApplicationIdAsync(Guid applicationId);
        Task<List<UserApplication>> GetByUserIdAsync(Guid userId);
        Task<bool> ExistsAsync(Guid userId, Guid applicationId);
        Task<(List<UserApplication> Items, int TotalRows)> GetUserApplicationPagedAsync(
               Expression<Func<UserApplication, bool>> filter = null,
               Func<IQueryable<UserApplication>, IOrderedQueryable<UserApplication>> orderBy = null,
               int pageNumber = 0,
               int pageSize = 0
        );
    }
}
