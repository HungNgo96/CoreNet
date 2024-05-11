// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Core.SharedKernel;
using Domain.Entities.Products;
using Domain.Shared;

namespace Application.Products.Commands.UpdateProduct
{
    public sealed class UpdateProductCommandHandler
        : ICommandHandler<UpdateProductCommand, IResult<bool>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductCommandHandler(IRepository<Product> productRepository,
                                           IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IResult<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.FindByIdAsync(request.Id, cancellationToken);

            if (product is null)
            {
                return Result<bool>.Fail("Not found production");
            }

            var updateProduct = Product.Update(product, request.Id, request.Name, request.Price, Sku.Create(request.Sku));
            _productRepository.Update(updateProduct);

            int countSave = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (countSave > 0)
            {
                return Result<bool>.Success(true, message: "Update success");
            }

            return Result<bool>.Fail("Update fail");
        }
    }
}
