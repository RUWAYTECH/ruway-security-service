using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SecurityMicroservice.Infrastructure.Data;
using SecurityMicroservice.Infrastructure.IRepositories;

namespace SecurityMicroservice.Infrastructure.Repositories
{
    public class EFRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public SecurityDbContext Db;
        internal DbSet<TEntity> DbSet;

        public EFRepository(SecurityDbContext context)
        {
            Db = context;
            DbSet = context.Set<TEntity>();
        }

        public virtual TEntity GetByKey(params object[] keyValues)
        {
            return DbSet.Find(keyValues);
        }

        public virtual async Task<TEntity> GetByKeyAsync(params object[] keyValues) => await DbSet.FindAsync(keyValues);

        public virtual TEntity GetFirstOrDefault(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = CreateDbSetQuery(filter, includeProperties);
            if (orderBy != null)
                return orderBy(query).FirstOrDefault();

            return query.FirstOrDefault();
        }

        public virtual Task<TEntity> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = CreateDbSetQuery(filter, includeProperties);
            return orderBy != null ? orderBy(query).FirstOrDefaultAsync(cancellationToken) : query.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = CreateDbSetQuery(filter, includeProperties);
            if (orderBy != null)
                return orderBy(query).AsQueryable();

            return query;
        }

        public virtual Task<List<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = CreateDbSetQuery(filter, includeProperties);
            if (orderBy != null)
                return orderBy(query).ToListAsync();

            return query.ToListAsync();
        }

        public virtual List<TEntity> GetPaged(
           out int rowsCount,
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           int pageNumber = 0,
           int pageSize = 0,
           params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = CreateDbSetQuery(filter, includeProperties);
            if (orderBy != null)
                query = orderBy(query);

            return ApplyQueryPagination(query, pageNumber, pageSize, out rowsCount);
        }

        public virtual void Insert(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            if (Db.Entry(entityToUpdate).State == EntityState.Detached)
                DbSet.Attach(entityToUpdate);

            Db.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void UpdatePartial(TEntity entityToUpdate, params string[] changedPropertyNames)
        {
            var entityEntry = Db.Entry(entityToUpdate);
            if (entityEntry.State != EntityState.Detached)
                entityEntry.State = EntityState.Detached;

            DbSet.Attach(entityToUpdate);

            foreach (var propertyName in changedPropertyNames)
                Db.Entry(entityToUpdate).Property(propertyName).IsModified = true;
        }

        public void UpdatePartialExcluding(TEntity entity, params string[] noChangedPropertyNames)
        {
            var entityEntry = Db.Entry(entity);
            if (entityEntry.State == EntityState.Detached)
                DbSet.Attach(entity);

            entityEntry.State = EntityState.Modified;

            foreach (var propertyName in noChangedPropertyNames)
                Db.Entry(entity).Property(propertyName).IsModified = false;
        }

        public virtual void Delete(params object[] keyValues)
        {
            TEntity entityToDelete = GetByKey(keyValues);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (Db.Entry(entityToDelete).State == EntityState.Detached)
                DbSet.Attach(entityToDelete);

            Db.Entry(entityToDelete).State = EntityState.Deleted;
            DbSet.Remove(entityToDelete);
        }

        public virtual TEntity FindByAltKey(params object[] keys)
        {
            return GetByKey(keys);
        }

        public IQueryable<TEntity> CreateDbSetQuery(
             Expression<Func<TEntity, bool>> filter = null,
             Expression<Func<TEntity, object>>[] includeProperties = null)
        {
            var query = DbSet.AsNoTracking();
            query = AddFilter(query, filter);
            query = AddIncludeProperties(query, includeProperties);
            return query;
        }

        private IQueryable<TEntity> AddFilter(
            IQueryable<TEntity> query,
            Expression<Func<TEntity, bool>> filter)
        {
            if (filter != null)
                query = query.Where(filter);
            return query;
        }

        private IQueryable<TEntity> AddIncludeProperties(
            IQueryable<TEntity> query,
            Expression<Func<TEntity, object>>[] includeProperties)
        {
            if (includeProperties != null)
                query = includeProperties.Aggregate(query, (current, include) => current.Include(include));
            return query;
        }

        private List<TEntity> ApplyQueryPagination(IQueryable<TEntity> query, int pageNumber, int pageSize, out int rowsCount)
        {
            if (pageSize > 0 && pageNumber > 0)
            {
                rowsCount = query.Count();
                var totalPages = Math.Ceiling((decimal)(rowsCount / pageSize));
                if (pageNumber > totalPages)
                    pageNumber = (int)totalPages;
                return query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            var result = query.ToList();
            rowsCount = result.Count();
            return result;
        }

        public virtual async Task<IList<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return await CreateDbSetQuery(includeProperties: includeProperties).ToListAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual bool Any(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.Any(predicate);
        }

        public void Dispose()
        {
            Db.Dispose();
            GC.SuppressFinalize(this);
        }


        public virtual async Task<(List<TEntity> Items, int TotalRows)> GetPagedAsync(
                 Expression<Func<TEntity, bool>> filter = null,
                 Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                 int pageNumber = 0,
                 int pageSize = 0,
                 params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = CreateDbSetQuery(filter, includeProperties);
            if (orderBy != null)
                query = orderBy(query);

            int rowsCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, rowsCount);
        }
        
    }
}
