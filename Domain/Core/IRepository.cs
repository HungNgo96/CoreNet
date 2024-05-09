// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.Specification;
using Domain.Primitives;

namespace Domain.Core
{
    public interface IRepository<TEntity> where TEntity : BaseEntity, IAggregateRoot
    {
        Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<TEntity?> FindOneAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken);

        Task<List<TEntity>> FindAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken);

        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);

        TEntity Update(TEntity entity);

        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken);
    }
}
