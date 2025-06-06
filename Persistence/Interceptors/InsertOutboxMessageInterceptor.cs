﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.Abstractions;
using Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace Persistence.Interceptors
{
    public sealed class InsertOutboxMessageInterceptor : SaveChangesInterceptor
    {
        private static readonly JsonSerializerSettings s_jsonSerializerSettings = new()
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

        private static void InsertOutboxMessage(DbContext context)
        {
            var utcNow = DateTime.UtcNow;

            var outboxMessage = context
                .ChangeTracker
                .Entries<EntityBase>()
                .Where(entityEntry => entityEntry.Entity.GetDomainEvents.Any())
                .Select(entry => entry.Entity)
                .SelectMany(entity =>
                {
                    var domainEvents = entity.GetDomainEvents.ToList();

                    entity.ClearDomainEvents();

                    return domainEvents;
                })
                .Select(domainEvent => new OutboxMessage()
                {
                    Id = Guid.NewGuid(),
                    Content = JsonConvert.SerializeObject(domainEvent, s_jsonSerializerSettings),
                    OccurredOnUtc = utcNow,
                    Type = domainEvent.GetType().Name,
                }).ToList();

            context.Set<OutboxMessage>().AddRange(outboxMessage);
        }
    }
}
