// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.Events;

namespace Domain.Events
{
    public class RemoveProductDomainEvent : IDomainEvent
    {
        public RemoveProductDomainEvent(Guid productId)
        {
            ProductId = productId;
        }

        public Guid ProductId { get; }
    }
}
