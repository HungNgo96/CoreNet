// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Domain.Entities.Products;

public class Product
{
    public ProductId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Money Price { get; private set; }

    public Sku Sku { get; private set; }
}

public record ProductId(Guid Id);
