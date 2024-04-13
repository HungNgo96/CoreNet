// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Data;
using Domain.Entities.Products;
using Domain.Repositories;
using Domain.Shared;

namespace Application.Products.Commands.CreateProduct
{
    public sealed class CreateProductCommandHandler
        : ICommandHandler<CreateProductCommand, IResult<bool>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IResult<bool>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = Product.Create(request.Id, request.Name, request.Price, Sku.Create(request.Sku));

            await _productRepository.InsertAsync(product, cancellationToken);

            int countSave = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (countSave > 0)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Fail("Insert fail");
        }
    }
}
