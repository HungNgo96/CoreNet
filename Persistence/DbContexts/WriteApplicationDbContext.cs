// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Data;
using Domain.Core.Abstractions;
using Domain.Core.Extensions;
using Domain.Core.SharedKernel;
using Domain.Entities.Customers;
using Domain.Entities.Orders;
using Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Persistence.Idempotency;
using Persistence.Outbox;

namespace Persistence.DbContexts
{
    public class WriteApplicationDbContext : DbContext, IWriteApplicationDbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;
        private readonly ILogger<WriteApplicationDbContext> _logger;

        public WriteApplicationDbContext(DbContextOptions<WriteApplicationDbContext> options,
                                         IMediator mediator,
                                         ILogger<WriteApplicationDbContext> logger) : base(options)
        {
            _mediator = mediator;
            _logger = logger;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemlyReference).Assembly);
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderSummary> OrderSummaries { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<LineItem> LineItems { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<IdempotentRequest> IdempotentRequests { get; set; }

        /// <summary>
        /// Saves all of the pending changes in the unit of work.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of entities that have been saved.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var strategy = Database.CreateExecutionStrategy();
            var rowsAffected = 0;
            // Executing the strategy.
            await strategy.ExecuteAsync(cancellationToken, async (token) =>
            {
                await using var transaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);

                _logger.Info(nameof(WriteApplicationDbContext), nameof(SaveChangesAsync), $"----- Begin transaction: '{transaction.TransactionId}'");

                try
                {
                    var utcNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    UpdateAuditableEntities(utcNow);

                    UpdateSoftDeletableEntities(utcNow);

                    //await PublishDomainEvents(cancellationToken);
                    // Getting the domain events and event stores from the tracked entities in the EF Core context.
                    rowsAffected = await base.SaveChangesAsync(token);

                    _logger.LogInformation("----- Commit transaction: '{TransactionId}'", transaction.TransactionId);

                    await transaction.CommitAsync(cancellationToken);

                    // Triggering the events and saving the stores.

                    _logger.Info(nameof(WriteApplicationDbContext), nameof(SaveChangesAsync),
                        $"----- Transaction successfully confirmed: '{transaction.TransactionId}', Rows Affected: {rowsAffected}");
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        nameof(WriteApplicationDbContext), nameof(SaveChangesAsync),
                        $"An unexpected exception occurred while committing the transaction: '{transaction.TransactionId}', message: {ex.Message}", ex);

                    await transaction.RollbackAsync(cancellationToken);

                    throw;
                }
            });

            return rowsAffected;
        }

        /// <inheritdoc />
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Database.BeginTransactionAsync(cancellationToken);

        /// <summary>
        /// Updates the entities implementing <see cref="IAuditableEntity"/> interface.
        /// </summary>
        /// <param name="utcNow">The current date and time in UTC format.</param>
        private void UpdateAuditableEntities(long utcNow)
        {
            foreach (var entityEntry in ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entityEntry.State == EntityState.Added)
                {
                    entityEntry.Property(nameof(IAuditableEntity.CreatedOnUtc)).CurrentValue = utcNow;
                }

                if (entityEntry.State == EntityState.Modified)
                {
                    entityEntry.Property(nameof(IAuditableEntity.ModifiedOnUtc)).CurrentValue = utcNow;
                }
            }
        }

        /// <summary>
        /// Updates the entities implementing <see cref="ISoftDeletableEntity"/> interface.
        /// </summary>
        /// <param name="utcNow">The current date and time in UTC format.</param>
        private void UpdateSoftDeletableEntities(long utcNow)
        {
            foreach (var entityEntry in ChangeTracker.Entries<ISoftDeletableEntity>())
            {
                if (entityEntry.State != EntityState.Deleted)
                {
                    continue;
                }

                entityEntry.Property(nameof(ISoftDeletableEntity.DeletedOnUtc)).CurrentValue = utcNow;

                entityEntry.Property(nameof(ISoftDeletableEntity.Deleted)).CurrentValue = true;

                entityEntry.State = EntityState.Modified;

                UpdateDeletedEntityEntryReferencesToUnchanged(entityEntry);
            }
        }

        /// <summary>
        /// Updates the specified entity entry's referenced entries in the deleted state to the modified state.
        /// This method is recursive.
        /// </summary>
        /// <param name="entityEntry">The entity entry.</param>
        private static void UpdateDeletedEntityEntryReferencesToUnchanged(EntityEntry entityEntry)
        {
            if (!entityEntry.References.Any())
            {
                return;
            }

            foreach (var targetEntry in entityEntry.References.Where(r => r.TargetEntry!.State == EntityState.Deleted).Select(x => x.TargetEntry))
            {
                targetEntry!.State = EntityState.Unchanged;

                UpdateDeletedEntityEntryReferencesToUnchanged(targetEntry);
            }
        }

        /// <summary>
        /// Publishes and then clears all the domain events that exist within the current transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task PublishDomainEvents(CancellationToken cancellationToken)
        {
            var aggregateRoots = ChangeTracker
                .Entries<EntityBase>()
                .Where(entityEntry => entityEntry.Entity.GetDomainEvents.Any())
                .ToList();

            var domainEvents = aggregateRoots.SelectMany(entityEntry => entityEntry.Entity.GetDomainEvents).ToList();

            aggregateRoots.ForEach(entityEntry => entityEntry.Entity.ClearDomainEvents());

            var tasks = domainEvents.Select(domainEvent => _mediator.Publish(domainEvent, cancellationToken));

            await Task.WhenAll(tasks);
        }
    }
}
