// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Primitives;

namespace Domain.Entities.Products;

public class Product : AggregateRoot
{
    public new ProductId? Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Money? Price { get; private set; }

    public Sku? Sku { get; private set; }
}
