// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core;
using Domain.Core.Abstractions;

namespace Domain.Entities.Orders
{
    public class OrderSummary : EntityBase
    {
        public long CustomerId { get; private set; }
        public decimal TotalPrice { get; private set; } = 0;

        public OrderSummary()
        { }

        public static OrderSummary Create(long customerId, decimal totalPrice)
        {
            return new OrderSummary()
            {
                Id = NumericIdGenerator.Generate(),
                CustomerId = customerId,
                TotalPrice = totalPrice
            };
        }
    }
}
