// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Abstractions.Messaging;
using Domain.Entities.Products;
using Domain.Repositories;
using Domain.Shared;

namespace Application.Products.Queries.GetProduct
{
    public sealed class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, IResult<Product?>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IResult<Product?>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

            if (product is null)
            {
                return Result<Product?>.Fail("Data not found");
            }

            return Result<Product?>.Success(data: product);
        }
    }
}
