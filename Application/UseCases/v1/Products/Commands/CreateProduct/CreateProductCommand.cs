// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Common.Shared;
using Contract.Abstractions.EventBus;
using Contract.Abstractions.Idempotency;
using Contract.Abstractions.Messaging;
using Domain.Core;
using Domain.Core.SharedKernel;
using Domain.Entities.Products;
using Domain.Repositories;
using FluentValidation;

namespace Application.UseCases.v1.Products.Commands.CreateProduct
{
    public static class CreateProduct
    {
        public sealed record Command : IdempotentCommand, ICommand<IResult<bool>>
        {
            public string Name { get; init; } = string.Empty;
            public Money? Price { get; init; }

            public string Sku { get; init; } = string.Empty;
        }

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                ///_ = RuleFor(x => x.Id).Must(x => !string.IsNullOrEmpty(x.ToString())).WithMessage("Id invalid");
                _ = RuleFor(x => x.Name).Must(x => !string.IsNullOrEmpty(x.ToString())).WithMessage("Name invalid");
                _ = RuleFor(x => x.Sku).Must(x => int.TryParse(x, out var _)).WithMessage("Sku invalid");
                _ = RuleFor(x => x.Price)
                    .Must(x => x?.Amount != 0).WithMessage("Amount of Sku invalid")
                    .Must(x => !string.IsNullOrEmpty(x!.Currency)).WithMessage("Currency of Sku invalid");
            }
        }

        public sealed class Handler(IProductRepository productRepository,
                                    IUnitOfWork unitOfWork,
                                    IEventBus eventBus) : ICommandHandler<Command, IResult<bool>>
        {
            public async Task<IResult<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                var product = Product.Create(NumericIdGenerator.Generate(), request.Name, request.Price, Sku.Create(request.Sku));

                await productRepository.InsertAsync(product, cancellationToken).ConfigureAwait(false);

                var countSave = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (countSave == 0)
                {
                    return Result<bool>.Fail("Insert fail");
                }

                var tasks = new List<Task>();

                for (var i = 0; i < 2; i++)
                {
                    tasks.Add(eventBus.PublishAsync(new ProductCreatedEvent()
                    {
                        Id = product.Id,
                        Name = product.Name + "-" + i,
                        Price = product.Price?.Amount ?? 0
                    }, cancellationToken));
                }

                await Task.WhenAll(tasks);

                return Result<bool>.Success(true);
            }
        }
    }
}
