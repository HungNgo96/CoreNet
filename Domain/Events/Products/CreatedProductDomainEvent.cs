﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.Events;
using Domain.Entities.Products;

namespace Domain.Events.Products
{
    public sealed record CreatedProductDomainEvent(long Id, string Name, Money? Price, Sku? Sku)
        : IDomainEvent
    {
        public long Id { get; set; } = Id;
        public string Name { get; set; } = Name;
        public Money? Price { get; set; } = Price;

        public Sku? Sku { get; set; } = Sku;
    }
}
