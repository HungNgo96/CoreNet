// HungNgo96

namespace Domain.Core.Abstractions
{
    public abstract class EntityBase<TKey> : IEntityBase<TKey>
    {
        public TKey Id { get; }
    }
}
