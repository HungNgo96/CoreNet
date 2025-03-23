// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Entities.Customers;
using Domain.Entities.Orders;
using Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace Contract.Interfaces.Persistence
{
    public interface IApplicationDbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderSummary> OrderSummaries { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<LineItem> LineItems { get; set; }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
