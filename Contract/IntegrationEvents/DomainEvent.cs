﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Contract.Abstractions.Messaging;
using MassTransit;

namespace Contract.IntegrationEvents
{
    public static class DomainEvent
    {
        public record class EmailNotificationEvent : INotificationEvent
        {
            public required string Name { get; set; }
            public required string Description { get; set; }
            public required string Type { get; set; }

            public Guid TransactionId { get; set; }
            public Guid Id { get; set; }
            public DateTimeOffset TimeStamp { get; set; }
        }

        public record class SmsNotificationEvent : INotificationEvent
        {
            public required string Name { get; set; }
            public required string Description { get; set; }
            public required string Type { get; set; }

            public Guid TransactionId { get; set; }
            public Guid Id { get; set; }
            public DateTimeOffset TimeStamp { get; set; }
        }
    }

    [EntityName("product-receive-endpoint")]
    public class ProductReceiveEndpoint
    {
        public Guid TransactionId { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
