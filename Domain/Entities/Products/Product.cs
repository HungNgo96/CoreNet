// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Core.Abstractions;
using Domain.Core.SharedKernel;
using Domain.Events.Products;

namespace Domain.Entities.Products;

public class Product : EntityBase, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    public Money? Price { get; private set; }

    public Sku? Sku { get; private set; }

    public Product()
    {
    }

    public Product(long id, string name, Money? price, Sku? sku) : base(id)
    {
        Name = name;
        Price = price;
        Sku = sku;
    }

    public static Product Create(long id, string name, Money? price, Sku? sku)
    {
        var product = new Product(id, name, price, sku);

        product.AddDomainEvent(new CreatedProductDomainEvent(id, name, price, sku));

        return product;
    }

    public static Product Update(Product product, long id, string name, Money? price, Sku? sku)
    {
        product.Name = name;
        product.Price = price;
        product.Sku = sku;

        product.AddDomainEvent(new UpdatedProductDomainEvent(id, name, price, sku));

        return product;
    }

    public static Product Delete(Product product, long id)
    {
        product.AddDomainEvent(new DeletedProductDomainEvent(id));

        return product;
    }
}
