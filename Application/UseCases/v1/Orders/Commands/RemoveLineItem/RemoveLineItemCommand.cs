// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Contract.Abstractions.Messaging;
using Domain.Entities.Orders;

namespace Application.UseCases.v1.Orders.Commands.RemoveLineItem
{
    public record RemoveLineItemCommand(OrderId OrderId, LineItemId LineItemId) : ICommand<bool>;
}
