// HungNgo96

using Application.Abstractions.Messaging;
using Domain.Core.SharedKernel;
using Domain.Entities.Products;
using Domain.Shared;
using FluentValidation;

namespace Application.Products.Commands.DeleteProduct
{
    public sealed class DeleteProductCommand
    {
        public sealed record Command(Guid Id) : ICommand<IResult<bool>>;

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                //_ = RuleFor(x => x.id is Guid);
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

                var updateProduct = Product.Delete(request.Id);

                await productRepository.RemoveAsync(updateProduct, cancellationToken).ConfigureAwait(false);

                int countSave = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (countSave > 0)
                {
                    return Result<bool>.Success(true, message: "Update success");
                }

                return Result<bool>.Fail("Update fail");
            }
        }

    }
}
