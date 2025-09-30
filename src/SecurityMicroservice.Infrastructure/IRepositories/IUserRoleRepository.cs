using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.IRepositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<List<UserRole>> GetByUserIdAsync(Guid userId);
        Task<List<UserRole>> GetByRoleIdAsync(Guid roleId);
        Task<bool> ExistsAsync(Guid userId, Guid roleId);
    }
}
