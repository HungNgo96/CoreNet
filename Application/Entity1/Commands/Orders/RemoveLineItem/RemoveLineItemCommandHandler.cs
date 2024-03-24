// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Application.Entity1.Commands.Orders.RemoveLineItem
{
    public sealed class RemoveLineItemCommandHandler : ICommandHandler<RemoveLineItemCommand, bool>
    {
        private readonly IWriteApplicationDbContext _context;

        public RemoveLineItemCommandHandler(IWriteApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(RemoveLineItemCommand request, CancellationToken cancellationToken)
        {
            var order = await _context!
                .Orders!
                .Include(o => o.LineItems.Where(li => li.Id == request.LineItemId))
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
            {
                return false;
            }

             order.RemoveLineItem(request.LineItemId);

            return await _context.SaveChangesAsync(cancellationToken) > 0;

        }
    }
}
