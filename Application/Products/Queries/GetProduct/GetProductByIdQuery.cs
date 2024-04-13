// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Abstractions.Messaging;
using Domain.Entities.Products;
using Domain.Shared;

namespace Application.Products.Queries.GetProduct
{
    public record GetProductByIdQuery(Guid Id) : IQuery<IResult<Product?>>;

}
