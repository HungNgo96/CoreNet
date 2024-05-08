// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Primitives;

namespace Domain.Core
{
    public interface IRepository<TEntity> where TEntity : BaseEntity, IAggregateRoot
    {
        //TEntity FindById(Guid id);
        //Task<TEntity> FindOneAsync(ISpecification<TEntity> spec);
        //Task<List<TEntity>> FindAsync(ISpecification<TEntity> spec);
        //Task<TEntity> AddAsync(TEntity entity);
        //Task RemoveAsync(TEntity entity);
    }
}
