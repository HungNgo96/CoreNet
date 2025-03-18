// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Application.UseCases.v1.Products.Commands.CreateProduct
{
    public record ProductCreatedEvent
    {
        public long Id { get; init; }
        public required string Name { get; init; }
        public decimal Price { get; init; }
    }
}
