// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.SharedKernel;
using Domain.Entities.Products;

namespace Domain.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        public Task<List<Product>> GetAsync(CancellationToken cancellationToken);

        public Task InsertAsync(Product product, CancellationToken cancellationToken);
    }
}
