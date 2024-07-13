// HungNgo96

namespace Domain.Core.Abstractions
{
    public interface IEntityBase<out TKey>
    {
        public TKey Id { get; }
    }
}
