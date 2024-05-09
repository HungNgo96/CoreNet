using Domain.Core;
using Domain.Core.Specification;
using Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RepositoryBase<TDbContext, TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity, IAggregateRoot
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        protected RepositoryBase(TDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TEntity>().AsNoTracking().SingleOrDefaultAsync(e => e.Id == id, cancellationToken)!;
        }

        public async Task<TEntity?> FindOneAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken)
        {
            var specificationResult = GetQuery(_dbContext.Set<TEntity>(), spec);

            return await specificationResult.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TEntity>> FindAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken)
        {
            var specificationResult = GetQuery(_dbContext.Set<TEntity>(), spec);

            return await specificationResult.ToListAsync();
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity);

            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            _dbContext.Set<TEntity>().Update(entity);

            return entity;
        }

        public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _dbContext.Set<TEntity>().Remove(entity);

            await _dbContext.SaveChangesAsync();
        }

        private static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> specification)
        {
            var query = inputQuery;

            if (specification.Criteria is not null)
            {
                query = query.Where(specification.Criteria);
            }

            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

            query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

            if (specification.OrderBy is not null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending is not null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            if (specification.GroupBy is not null)
            {
                query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
            }

            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip - 1)
                    .Take(specification.Take);
            }

            return query;
        }
    }
}
