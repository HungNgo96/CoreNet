// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Contract.Interfaces.Persistence;
using Domain.Core.SharedKernel;
using Domain.Entities.Products;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence.DbContexts;
using Persistence.Repositories.Commons;

namespace Persistence.Repositories
{
    public sealed class ProductRepository(
        IReadApplicationDbContext readDbContext,
        IRepository<Product> productRepository,
        WriteApplicationDbContext writeApplicationDbContext)
        : RepositoryBase<WriteApplicationDbContext, Product>(writeApplicationDbContext), IProductRepository
    {
        public Task<List<Product>> GetAsync(CancellationToken cancellationToken)
        {
            var queryable = readDbContext.Products.Where(x => !string.IsNullOrEmpty(x.Id.ToString()));

            IQueryable<Product> orderBy = queryable.OrderBy(x => x.Name);

            return orderBy.ToListAsync(cancellationToken);
        }

        public Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            return readDbContext.Products.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public Task InsertAsync(Product product, CancellationToken cancellationToken)
        {
            return productRepository.AddAsync(product, cancellationToken);
        }
    }
}
