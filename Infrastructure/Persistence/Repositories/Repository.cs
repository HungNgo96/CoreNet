// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.SharedKernel;
using Domain.Primitives;

namespace Infrastructure.Persistence.Repositories
{
    public class Repository<TEntity>
        : RepositoryBase<WriteApplicationDbContext, TEntity>
        where TEntity : BaseEntity, IAggregateRoot
    {
        public Repository(WriteApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
