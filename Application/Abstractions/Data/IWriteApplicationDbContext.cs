// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data
{
    public interface IWriteApplicationDbContext
    {
        public DbSet<Order> Orders { get; set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
