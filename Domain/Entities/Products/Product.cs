// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Core.Abstractions;
using Domain.Core.SharedKernel;
using Domain.Events;
using Domain.Events.Products;
using Domain.Primitives;

namespace Domain.Entities.Products;

public class Product : BaseEntity, IAggregateRoot, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public Money? Price { get; private set; }

    public Sku? Sku { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public Product()
    {
    }

    public Product(Guid id, string name, Money? price, Sku? sku) : base(id)
    {
        Name = name;
        Price = price;
        Sku = sku;
    }

    public static Product Create(Guid id, string name, Money? price, Sku? sku)
    {
        var product = new Product(id, name, price, sku);

        product.AddDomainEvent(new CreatedProductDomainEvent(id, name, price, sku));

        return product;
    }

    public static Product Update(Product product, Guid id, string name, Money? price, Sku? sku)
    {
        product.Name = name;
        product.Price = price;
        product.Sku = sku;

        product.AddDomainEvent(new UpdatedProductDomainEvent(id, name, price, sku));

        return product;
    }
}
