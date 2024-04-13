// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Abstractions.Messaging;
using Domain.Shared;

namespace Application.Products.Commands.CreateProduct
{
    public record CreateProductCommand : ICommand<IResult<bool>>
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public Money? Price { get; init; }

        public string Sku { get; init; }
    }
}
