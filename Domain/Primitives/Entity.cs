// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.Events;
using Domain.Core.SharedKernel;
using Domain.Core.Utility;

namespace Domain.Primitives
{
    public abstract class BaseEntity : IEntity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        protected BaseEntity() => Id = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <remarks>
        /// Required by EF Core.
        /// </remarks>
        protected BaseEntity(Guid id) => Id = id;
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public Guid Id { get; private set; }

        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

        /// <summary>
        /// Gets the domain events. This collection is readonly.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> GetDomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Clears all the domain events from the <see cref="AggregateRoot"/>.
        /// </summary>
        public void ClearDomainEvents() => _domainEvents.Clear();

        /// <summary>
        /// Adds the specified <see cref="IDomainEvent"/> to the <see cref="AggregateRoot"/>.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    }
}
