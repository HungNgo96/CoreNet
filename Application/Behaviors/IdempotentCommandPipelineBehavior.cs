// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Contract.Abstractions.Idempotency;
using MediatR;

namespace Application.Behaviors
{
    public sealed class IdempotentCommandPipelineBehavior<TRequest, TResponse>(IIdempotencyService idempotentService)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IdempotentCommand
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (await idempotentService.RequestExistAsync(request.RequestId, cancellationToken).ConfigureAwait(false))
            {
                return default!;
            }

            await idempotentService.CreateRequestAsync(requestId: request.RequestId, typeof(TRequest).Name, cancellationToken).ConfigureAwait(false);

            var response = await next();

            return response;
        }
    }
}
