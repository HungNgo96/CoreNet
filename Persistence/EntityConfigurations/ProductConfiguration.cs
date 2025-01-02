// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Extensions;

namespace Persistence.EntityConfigurations
{
    internal class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ConfigureBaseEntity();
            //builder.Property(p => p.Id).HasConversion(
            //    productId => productId.Value,
            //    value => new ProductId(value));

            builder.Property(p => p.Name).HasMaxLength(100);

            _ = builder.Property(p => p.Sku).HasConversion(
               skuId => skuId.Value,
               value => Sku.Create(value));

            builder.OwnsOne(p => p.Price, priceBuilder =>
            {
                priceBuilder.Property(m => m.Currency).HasMaxLength(3);
                priceBuilder.Property(m => m.Amount).HasColumnType(nameof(Decimal));
            });
        }
    }
}
