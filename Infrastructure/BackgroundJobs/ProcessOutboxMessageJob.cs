// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Data;
using Domain.Core.Events;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;

namespace Infrastructure.BackgroundJobs
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
            List<OutboxMessage> outboxMessages = await _context
               .OutboxMessages
               .Where(x => x.ProcessedOnUtc == null)
               .Take(20)
               .ToListAsync(context.CancellationToken);

            foreach (OutboxMessage outboxMessage in outboxMessages)
            {
                IDomainEvent? domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(outboxMessage.Content,
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
