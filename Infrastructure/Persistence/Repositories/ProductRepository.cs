// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;
using Application.Abstractions.Data;
using Domain.Entities.Products;
using Domain.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class ProductRepository : IProductRepository, IScopeService
    {
        private readonly IReadApplicationDbContext _readDbContext;
        private readonly IWriteApplicationDbContext _writeDbContext;

        public ProductRepository(IReadApplicationDbContext readDbContext,
                                 IWriteApplicationDbContext writeDbContext)
        {
            _readDbContext = readDbContext;
            _writeDbContext = writeDbContext;
        }

        public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken)
        {
            IQueryable<Product> iqueryAble = _readDbContext.Products.Where(x => !string.IsNullOrEmpty(x.Id.ToString()));

            IQueryable<Product> orderBy = iqueryAble.OrderBy(x => x.Name);
            IQueryable<Product> iqueryAble2 = orderBy.Skip(3).Take(2);
            //Func<Product, bool> d = (x) => !string.IsNullOrEmpty(x.Name);
            //IReadOnlyCollection<Product> result = _readDbContext.Products.Where(d).ToList().AsReadOnly();
            return await iqueryAble2.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Expression<Func<Product, bool>> condition = p => p.Id == id;

            return await _readDbContext.Products.FirstOrDefaultAsync(condition, cancellationToken).ConfigureAwait(false);
        }

        public async Task InsertAsync(Product product, CancellationToken cancellationToken)
        {
            await _writeDbContext.Products.AddAsync(product, cancellationToken);
        }
    }
}
