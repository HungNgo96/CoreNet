using System.Linq.Expressions;
using Domain.Core.Abstractions;
using Domain.Core.SharedKernel;
using Domain.Core.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace Persistence.Repositories.Commons
{
    public class RepositoryBase<TDbContext, TEntity> : IRepository<TEntity>
        where TEntity : EntityBase, IAggregateRoot
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        protected RepositoryBase(TDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TEntity?> FindByIdAsync(long id, CancellationToken cancellationToken)
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

            return await specificationResult.ToListAsync(cancellationToken);
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            _dbContext.Set<TEntity>().Update(entity);

            return entity;
        }

        public EntityEntry<TEntity> Remove(TEntity entity)
        {
            return _dbContext.Set<TEntity>().Remove(entity);
        }

        public Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbContext.Set<TEntity>().Where(predicate).ExecuteDeleteAsync();
        }

        public Task<int> ExecuteUpdateAsync(Expression<Func<TEntity, bool>> predicate,
                                            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls)
        {
            return _dbContext.Set<TEntity>().Where(predicate).ExecuteUpdateAsync(setPropertyCalls);
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
