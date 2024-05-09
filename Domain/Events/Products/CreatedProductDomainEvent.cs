// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.Events;
using Domain.Entities.Products;

namespace Domain.Events
{
    public sealed record CreatedProductDomainEvent : IDomainEvent
    {
        public CreatedProductDomainEvent(Guid id, string name, Money? price, Sku? sku)
        {
            Id = id;
            Name = name;
            Price = price;
            Sku = sku;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Money? Price { get; set; }

        public Sku? Sku { get; set; }
    }
}
