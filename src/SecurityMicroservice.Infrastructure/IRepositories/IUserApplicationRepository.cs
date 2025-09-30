using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.IRepositories
{
    public interface IUserApplicationRepository : IRepository<UserApplication>
    {
        Task<List<UserApplication>> GetByApplicationIdAsync(Guid applicationId);
        Task<List<UserApplication>> GetByUserIdAsync(Guid userId);
        Task<bool> ExistsAsync(Guid userId, Guid applicationId);
    }
}
