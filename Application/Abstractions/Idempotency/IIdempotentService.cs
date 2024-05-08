// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Services;
using Domain.Core;

namespace Application.Abstractions.Idempotency
{
    public interface IIdempotencyService : IScopeService
    {
        Task<bool> RequestExistAsync(Guid requestId, CancellationToken cancellationToken);

        Task CreateRequestAsync(Guid requestId, string requestName, CancellationToken cancellationToken);
    }
}
