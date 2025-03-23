// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Common.Shared;
using Contract.Abstractions.Messaging;
using Domain.Core.SharedKernel;
using Domain.Entities.Products;
using FluentValidation;

namespace Application.UseCases.v1.Products.Commands.UpdateProduct
{
    public sealed class UpdateProductCommand
    {
        public sealed record Command : ICommand<IResult<bool>>
        {
            public long Id { get; private set; }
            public string Name { get; init; } = string.Empty;
            public Money? Price { get; init; }

            public string Sku { get; init; } = string.Empty;

            public void SetId(long id) => Id = id;
        }

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                _ = RuleFor(x => x.Id).Must(x => !string.IsNullOrEmpty(x.ToString())).WithMessage("Id invalid");

                _ = RuleFor(x => x.Name).Must(x => !string.IsNullOrEmpty(x.ToString())).WithMessage("Name invalid");

                _ = RuleFor(x => x.Sku).Must(x => int.TryParse(x, out var _)).WithMessage("Sku invalid");

                _ = RuleFor(x => x.Price)
                    .Must(x => x?.Amount != 0).WithMessage("Amount of Sku invalid")
                    .Must(x => !string.IsNullOrEmpty(x!.Currency)).WithMessage("Currency of Sku invalid");
            }
        }

        public sealed class Handler(IRepository<Product> productRepository,
                                    IUnitOfWork unitOfWork) : ICommandHandler<Command, IResult<bool>>
        {
            public async Task<IResult<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                var product = await productRepository.FindByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

                if (product is null)
                {
                    return Result<bool>.Fail("Not found production");
                }

                var updateProduct = Product.Update(product, request.Id, request.Name, request.Price, Sku.Create(request.Sku));

                productRepository.Update(updateProduct);

                var countSave = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (countSave > 0)
                {
                    return Result<bool>.Success(true, message: "Update success");
                }

                return Result<bool>.Fail("Update fail");
            }
        }
    }
}
