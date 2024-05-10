// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.Events;
using Domain.Events;

namespace Application.Products.Events.UpdateProduct
{
    public class UpdatedProductDomainEventHandler : IDomainEventHandler<UpdatedProductDomainEvent>
    {
        public Task Handle(UpdatedProductDomainEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"update product message create domain event name: {notification.Name}");

            return Task.CompletedTask;
        }
    }
}
