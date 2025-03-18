// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Responses.Products;
using Contract.Abstractions.Messaging;
using Domain.Repositories;
using Domain.Shared;
using Mapster;

namespace Application.UseCases.v1.Products.Queries.GetProductById
{
    public sealed class GetProductById
    {
        public record Query(long Id) : IQuery<IResult<GetProductResponse>>;

        public sealed class Handler(IProductRepository productRepository) : IQueryHandler<Query, IResult<GetProductResponse?>>
        {
            public async Task<IResult<GetProductResponse?>> Handle(Query request, CancellationToken cancellationToken)
            {
                var product = await productRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

                if (product is null)
                {
                    return Result<GetProductResponse?>.Fail("Data not found");
                }

                return Result<GetProductResponse?>.Success(data: product.Adapt<GetProductResponse>());
            }
        }
    }
}
