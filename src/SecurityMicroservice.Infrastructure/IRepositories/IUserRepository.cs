using SecurityMicroservice.Domain.Entities;
using System.Linq.Expressions;

namespace SecurityMicroservice.Infrastructure.IRepositories
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<(List<User> Items, int TotalRows)> GetUserPagedAsync(
           Expression<Func<User, bool>> filter = null,
           Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null,
           string applicationCode = "",
           int pageNumber = 0,
           int pageSize = 0
        );
        Task<User?> GetByIdAsync(Guid userId);
        Task<User?> GetByUsernameAsync(string username);
        Task<List<string>> GetUserApplicationScopesAsync(Guid userId);
        Task<List<string>> GetUserRolesAsync(Guid userId);
        Task<List<string>> GetUserPermissionsAsync(Guid userId);
        Task<User?> GetByTokenAsync(string token);
        Task<List<Permission>> GetUserPermissionEntitiesAsync(Guid userId);
        Task<List<User>> GetByApplicationIdAsync(Guid applicationId);
    }
}
