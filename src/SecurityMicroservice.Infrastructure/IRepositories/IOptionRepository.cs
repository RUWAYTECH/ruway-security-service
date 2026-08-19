using SecurityMicroservice.Domain.Entities;

namespace SecurityMicroservice.Infrastructure.IRepositories;

public interface IOptionRepository : IRepository<Option>
{
    Task<List<Option>> GetByModuleIdAsync(Guid moduleId);
    Task<List<Option>> GetByApplicationCodeAsync(string applicationCode);
    Task<Option?> GetByCodeAsync(string code);
    Task<int> GetMaxOrderByModuleIdAsync(Guid moduleId);
}