// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Application.Abstractions.Idempotency;
using Application.Abstractions.Messaging;
using Domain.Shared;
using FluentValidation;

namespace Application.Products.Commands.CreateProduct
{
    public record CreateProductCommand : IdempotentCommand, ICommand<IResult<bool>>
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public Money? Price { get; init; }

        public string Sku { get; init; } = string.Empty;
    }

    public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            _ = RuleFor(x => x.Id).Must(x => !string.IsNullOrEmpty(x.ToString())).WithMessage("Id invalid");
            _ = RuleFor(x => x.Name).Must(x => !string.IsNullOrEmpty(x.ToString())).WithMessage("Name invalid");
            _ = RuleFor(x => x.Sku).Must(x => int.TryParse(x, out var _)).WithMessage("Sku invalid");
            _ = RuleFor(x => x.Price)
                .Must(x => x?.Amount != 0).WithMessage("Amount of Sku invalid")
                .Must(x => !string.IsNullOrEmpty(x!.Currency)).WithMessage("Currency of Sku invalid");
        }
    }

}
