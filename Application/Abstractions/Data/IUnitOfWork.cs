// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Abstractions.Data
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Saves all of the pending changes in the unit of work.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of entities that have been saved.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a transaction on the current unit of work.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The new database context transaction.</returns>
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
