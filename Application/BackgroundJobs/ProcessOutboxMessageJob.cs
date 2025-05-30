﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Contract.Interfaces.Persistence;
using Domain.Core.Events;
using Domain.Core.SharedKernel;
using Domain.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;

namespace Application.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public sealed class ProcessOutboxMessageJob : IJob
    {
        private readonly IWriteApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public ProcessOutboxMessageJob(IWriteApplicationDbContext context, IPublisher publisher, IUnitOfWork unitOfWork)
        {
            _context = context;
            _publisher = publisher;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var outboxMessages = await _context.Set<OutboxMessage>()
               .Where(x => x.ProcessedOnUtc == null)
               .OrderBy(x => x.Id)
               .Take(20)
               .ToListAsync(context.CancellationToken);

            foreach (var outboxMessage in outboxMessages)
            {
                var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(outboxMessage.Content,
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All,
                    });

                if (domainEvent is null)
                {
                    continue;
                }

                await _publisher.Publish(domainEvent, context.CancellationToken);

                outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
    }
}
