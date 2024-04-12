// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Entities.Products;
using Domain.Services;

namespace Domain.Repositories
{
    public interface IProductRepository : IScopeService
    {
        public Task<Product> GetByIdAsync(int id, CancellationToken cancellationToken);
        public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken);
        public Task<bool> InsertAsync(Product product, CancellationToken cancellationToken);
    }
}
