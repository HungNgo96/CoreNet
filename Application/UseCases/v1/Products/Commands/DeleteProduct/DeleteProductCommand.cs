﻿// HungNgo96

using Common.Shared;
using Contract.Abstractions.Messaging;
using Domain.Core.SharedKernel;
using Domain.Entities.Products;
using FluentValidation;

namespace Application.UseCases.v1.Products.Commands.DeleteProduct
{
    public static class DeleteProductCommand
    {
        public sealed record Command(long Id) : ICommand<IResult<bool>>;

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                _ = RuleFor(x => x.Id).NotEmpty();
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

                var updateProduct = Product.Delete(product, request.Id);

                productRepository.Remove(updateProduct);

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
