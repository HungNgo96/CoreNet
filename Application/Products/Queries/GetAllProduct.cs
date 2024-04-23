// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Messaging;
using Domain.Entities.Products;
using Domain.Repositories;
using Domain.Shared;
using FluentValidation;

namespace Application.Products.Queries
{
    public sealed class GetAllProduct
    {
        public record Query : IQuery<IResult<IReadOnlyCollection<Product>>>
        {

            internal class Validator : AbstractValidator<Query>
            {
                public Validator()
                {
                    //RuleFor(x => x.Page)
                    //    .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

                    //RuleFor(x => x.PageSize)
                    //    .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
                }
            }
            internal class Handler : IQueryHandler<Query, IResult<IReadOnlyCollection<Product>>>
            {
                private readonly IProductRepository _productRepository;

                public Handler(IProductRepository productRepository)
                {
                    _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository)); ;
                }

                public async Task<IResult<IReadOnlyCollection<Product>>> Handle(Query request, CancellationToken cancellationToken)
                {
                    var result = await _productRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                    return result.Count != 0
                        ? Result<IReadOnlyCollection<Product>>.Success(data: result)
                        : Result<IReadOnlyCollection<Product>>.Fail("data not found");
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                }
            }
        }

    }
}
