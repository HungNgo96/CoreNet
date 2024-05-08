// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;
using Application.Abstractions.Data;
using Domain.Core;
using Domain.Entities.Products;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly IReadApplicationDbContext _readDbContext;
        private readonly IWriteApplicationDbContext _writeDbContext;
        private readonly IRepository<Product> _productRepository;

        public ProductRepository(IReadApplicationDbContext readDbContext,
                                 IWriteApplicationDbContext writeDbContext,
                                 IRepository<Product> productRepository)
        {
            _readDbContext = readDbContext;
            _writeDbContext = writeDbContext;
            _productRepository = productRepository;
        }

        public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken)
        {
            IQueryable<Product> iqueryAble = _readDbContext.Products.Where(x => !string.IsNullOrEmpty(x.Id.ToString()));

            IQueryable<Product> orderBy = iqueryAble.OrderBy(x => x.Name);
            //IQueryable<Product> iqueryAble2 = orderBy.Skip(3).Take(2);
            //Func<Product, bool> d = (x) => !string.IsNullOrEmpty(x.Name);
            //IReadOnlyCollection<Product> result = _readDbContext.Products.Where(d).ToList().AsReadOnly();
            return await orderBy.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Expression<Func<Product, bool>> condition = p => p.Id == id;

            return await _readDbContext.Products.FirstOrDefaultAsync(condition, cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertAsync(Product product, CancellationToken cancellationToken)
        {
            _ = await _productRepository.AddAsync(product, cancellationToken);
        }
    }
}
