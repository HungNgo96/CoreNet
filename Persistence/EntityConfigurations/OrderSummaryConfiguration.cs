// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Extensions;

namespace Persistence.EntityConfigurations
{
    public class OrderSummaryConfiguration : IEntityTypeConfiguration<OrderSummary>
    {
        public void Configure(EntityTypeBuilder<OrderSummary> builder)
        {
            builder.ConfigureBaseEntity();

            _ = builder.Property(o => o.TotalPrice).HasColumnType(nameof(Decimal));
            _ = builder.Property(o => o.CustomerId);
        }
    }
}
