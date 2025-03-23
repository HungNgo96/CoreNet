// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Contract.Abstractions.Messaging;
using Contract.Interfaces.Persistence;
using Domain.Core.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.v1.Orders.Commands.RemoveLineItem
{
    public sealed class RemoveLineItemCommandHandler : ICommandHandler<RemoveLineItemCommand, bool>
    {
        private readonly IWriteApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveLineItemCommandHandler(IWriteApplicationDbContext context,
                                            IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
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

            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
