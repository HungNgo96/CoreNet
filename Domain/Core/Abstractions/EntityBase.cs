// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.Events;
using Domain.Core.SharedKernel;

namespace Domain.Core.Abstractions
{
    public abstract class EntityBase : IEntity<long>, IAuditableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see>
        ///     <cref>Entity</cref>
        /// </see>
        /// class.
        /// </summary>
        protected EntityBase() => Id = NumericIdGenerator.Generate();

        /// <summary>
        /// Initializes a new instance of the <see>
        ///     <cref>Entity</cref>
        /// </see>
        /// class.
        /// </summary>
        /// <remarks>
        /// Required by EF Core.
        /// </remarks>
        protected EntityBase(long id) => Id = id;

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public long Id { get; set; }

        private readonly List<IDomainEvent> _domainEvents = [];

        /// <summary>
        /// Gets the domain events. This collection is readonly.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> GetDomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Clears all the domain events from the <see>
        ///     <cref>AggregateRoot</cref>
        /// </see>
        /// .
        /// </summary>
        public void ClearDomainEvents() => _domainEvents.Clear();

        /// <summary>
        /// Adds the specified <see cref="IDomainEvent"/> to the <see>
        ///     <cref>AggregateRoot</cref>
        /// </see>
        /// .
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            if (_domainEvents.Contains(domainEvent))
            {
                return;
            }

            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Gets the created on date and time in UTC format.
        /// </summary>
        public long CreatedOnUtc { get; }

        /// <summary>
        /// Gets the user who created this entity.
        /// </summary>
        public string CreatedBy { get; } = string.Empty;

        /// <summary>
        /// Gets the modified on date and time in UTC format.
        /// </summary>
        public long? ModifiedOnUtc { get; }

        /// <summary>
        /// Gets the user who last modified this entity.
        /// </summary>
        public string? ModifiedBy { get; }
    }
}
