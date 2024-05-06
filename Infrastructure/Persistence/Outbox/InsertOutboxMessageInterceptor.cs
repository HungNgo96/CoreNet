// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.Events;
using Domain.Entities;
using Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace Infrastructure.Persistence.Outbox
{
    public sealed class InsertOutboxMessageInterceptor : SaveChangesInterceptor
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
        };

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
                                                                              InterceptionResult<int> result,
                                                                              CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                InsertOutboxMessage(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void InsertOutboxMessage(DbContext context)
        {
            DateTime utcNow = DateTime.UtcNow;

            var outboxMessage = context
                .ChangeTracker
                .Entries<BaseEntity>()
                .Where(entityEntry => entityEntry.Entity.GetDomainEvents.Any())
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    List<IDomainEvent> domainEvents = entity.GetDomainEvents.ToList();

                    entity.ClearDomainEvents();

                    return domainEvents;
                })
                .Select(domainEvent =>
                {
                    return new OutboxMessage()
                    {
                        Id = Guid.NewGuid(),
                        Content = JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings),
                        OccurrendOnUtc = utcNow,
                        Type = domainEvent.GetType().Name,
                    };
                }).ToList();

            context.Set<OutboxMessage>().AddRange(outboxMessage);
        }
    }
}
