﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Domain.Outbox
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime OccurredOnUtc { get; set; }
        public DateTime? ProcessedOnUtc { get; set; } = null;
        public string? Error { get; set; } = null;
    }
}
