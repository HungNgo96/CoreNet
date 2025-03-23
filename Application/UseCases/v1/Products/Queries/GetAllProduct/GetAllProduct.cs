// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Responses.Products;
using Common.Shared;
using Contract.Abstractions.Messaging;
using Domain.Repositories;
using FluentValidation;
using Mapster;

namespace Application.UseCases.v1.Products.Queries.GetAllProduct
{
    public sealed class GetAllProduct
    {
        public record Query : IQuery<IResult<IReadOnlyCollection<GetProductResponse>>>
        {
        }

        public sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                ///RuleFor(x => x.Page)
                ///    .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

                ///RuleFor(x => x.PageSize)
                ///    .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
            }
        }

        public sealed class Handler : IQueryHandler<Query, IResult<IReadOnlyCollection<GetProductResponse>>>
        {
            private readonly IProductRepository _productRepository;

            public Handler(IProductRepository productRepository)
            {
                _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository)); ;
            }

            public async Task<IResult<IReadOnlyCollection<GetProductResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = await _productRepository.GetAsync(cancellationToken).ConfigureAwait(false);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                return result.Count != 0
                    ? Result<IReadOnlyCollection<GetProductResponse>>.Success(data: result.Adapt<List<GetProductResponse>>())
                    : Result<IReadOnlyCollection<GetProductResponse>>.Fail("data not found");
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }
        }
    }
}
