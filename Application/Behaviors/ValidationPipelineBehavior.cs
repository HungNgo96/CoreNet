// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Domain.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Behaviors
{
    public sealed class ValidationPipelineBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : Result<TRequest>
        where TResponse : Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

        public async Task<TResponse> Handle(TRequest request,
                                      RequestHandlerDelegate<TResponse> next,
                                      CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                //This runs all the validation rules one by one returns the validation result
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                //Now, need to check for any failures
                var failures = validationResults.SelectMany(e => e.Errors).Where(f => f != null).ToList();
                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }

            return await next();
        }
    }
}
