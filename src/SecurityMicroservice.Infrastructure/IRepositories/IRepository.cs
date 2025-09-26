using System.Linq.Expressions;

namespace SecurityMicroservice.Infrastructure.IRepositories
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        TEntity GetByKey(params object[] keyValues);
        Task<TEntity> GetByKeyAsync(params object[] keyValues);
        TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, params Expression<Func<TEntity, object>>[] includeProperties);

        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includeProperties);
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, params Expression<Func<TEntity, object>>[] includeProperties);
        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, params Expression<Func<TEntity, object>>[] includeProperties);
        List<TEntity> GetPaged(out int rowsCount, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int pageNumber = 0, int pageSize = 0, params Expression<Func<TEntity, object>>[] includeProperties);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        bool Any(Expression<Func<TEntity, bool>> predicate);

        Task<IList<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includeProperties);

        void Insert(TEntity entity);
        void Update(TEntity entityToUpdate);

        void UpdatePartial(TEntity entityToUpdate, params string[] changedPropertyNames);

        void UpdatePartialExcluding(TEntity entity, params string[] noChangedPropertyNames);

        void Delete(params object[] keyValues);
        void Delete(TEntity entityToDelete);

       public Task<(List<TEntity> Items, int TotalRows)> GetPagedAsync(
                 Expression<Func<TEntity, bool>> filter = null,
                 Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                 int pageNumber = 0,
                 int pageSize = 0,
                 params Expression<Func<TEntity, object>>[] includeProperties);
    }
}
