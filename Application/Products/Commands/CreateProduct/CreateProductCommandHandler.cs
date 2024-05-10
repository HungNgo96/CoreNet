// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Abstractions.Data;
using Application.Abstractions.EventBus;
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
        private readonly IEventBus _eventBus;

        public CreateProductCommandHandler(IProductRepository productRepository,
                                           IUnitOfWork unitOfWork,
                                           IEventBus eventBus)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
        }

        public async Task<IResult<bool>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = Product.Create(request.Id, request.Name, request.Price, Sku.Create(request.Sku));
            await _productRepository.InsertAsync(product, cancellationToken);

            int countSave = await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (countSave > 0)
            {
                List<Task> tasks = new List<Task>();

                for (int i = 0; i < 2; i++)
                {
                    tasks.Add(_eventBus.PublishAsync(new ProductCreatedEvent()
                    {
                        Id = product.Id,
                        Name = product.Name + "-" + i,
                        Price = product.Price?.Amount ?? 0
                    }));
                }

                await Task.WhenAll(tasks);

                return Result<bool>.Success(true);
            }

            return Result<bool>.Fail("Insert fail");
        }
    }
}
