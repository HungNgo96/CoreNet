// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Responses.Products;
using Common.Shared;
using Contract.Abstractions.Messaging;
using Domain.Repositories;
using Mapster;

namespace Application.UseCases.v1.Products.Queries.GetProductById
{
    public static class GetProductByIdQuery
    {
        public record Query(long Id) : IQuery<IResult<GetProductResponse>>;

        public sealed class Handler(IProductRepository productRepository) : IQueryHandler<Query, IResult<GetProductResponse?>>
        {
            public async Task<IResult<GetProductResponse?>> Handle(Query request, CancellationToken cancellationToken)
            {
                var product = await productRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

                return product is null 
                    ? Result<GetProductResponse?>.Fail("Data not found") 
                    : Result<GetProductResponse?>.Success(data: product.Adapt<GetProductResponse>());
            }
        }
    }
}
