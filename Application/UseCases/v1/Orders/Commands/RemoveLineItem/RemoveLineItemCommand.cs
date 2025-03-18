// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Contract.Abstractions.Messaging;

namespace Application.UseCases.v1.Orders.Commands.RemoveLineItem
{
    public sealed record RemoveLineItemCommand(long OrderId, long LineItemId) : ICommand<bool>;
}
