// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.DomainEvents;

namespace Domain.Primitives
{
    public abstract class AggregateRoot : Entity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        protected AggregateRoot(Guid id) : base(id)
        {
        }

        //protected AggregateRoot()
        //{
        //}

        //public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();
        //public void ClearDomainEvents() => _domainEvents.Clear();
        //protected void RaiseDomainEvent(IDomainEvent domainEvent)
        //{
        //    _domainEvents.Add(domainEvent);
        //}

    }
}
