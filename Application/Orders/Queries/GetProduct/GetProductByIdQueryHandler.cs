// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Entities.Products;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.GetProduct
{
    public sealed class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, IResult<Product?>>
    {
        private readonly IReadApplicationDbContext _context;

        public GetProductByIdQueryHandler(IReadApplicationDbContext context)
        {
            _context = context;
        }   

        public async Task<IResult<Product?>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (product is null)
            {
                return Result<Product?>.Fail("Data not found");
            }

            return Result<Product?>.Success(data: product);
        }
    }
}
