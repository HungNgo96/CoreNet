// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Abstractions.Data;
using Application.Abstractions.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Idempotency
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IWriteApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        public IdempotencyService(IWriteApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public Task<bool> RequestExistAsync(Guid requestId, CancellationToken cancellationToken)
        {
            return _context.Set<IdempotentRequest>().AnyAsync(r => r.Id == requestId, cancellationToken);
        }

        public async Task CreateRequestAsync(Guid requestId, string requestName, CancellationToken cancellationToken)
        {
            var idempotentRequest = new IdempotentRequest()
            {
                Id = requestId,
                Name = requestName,
                CreatedOnUtc = DateTime.UtcNow,
            };

            await _context.Set<IdempotentRequest>().AddAsync(idempotentRequest, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
