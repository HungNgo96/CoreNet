// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Entities.Orders;

namespace Domain.Repositories
{
    public interface IOrderRepository
    {
        public Task<Order> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken);

        public Task<bool> InsertAsync(Order order, CancellationToken cancellationToken);
    }
}
