// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml.Linq;
using Domain.Events;
using Domain.Primitives;

namespace Domain.Entities.Products;

public class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public Money? Price { get; private set; }

    public Sku? Sku { get; private set; }

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

        product.AddDomainEvent(new CreatedProductDomainEvent(product));

        return product;
    }
}
