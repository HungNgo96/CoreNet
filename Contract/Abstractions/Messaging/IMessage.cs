// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MassTransit;

namespace Contract.Abstractions.Messages
{
    [ExcludeFromTopology]
    public interface IMessage
    {
        public Guid Id { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
