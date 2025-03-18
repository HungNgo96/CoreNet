using Domain.Core.Abstractions;

namespace Domain.Entities.Orders;

public class LineItem : EntityBase
{
    public LineItem(long id, long orderId, long productId, Money price)
    {
        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Price = price;
    }

    public LineItem()
    {
    }

    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public long ProductId { get; private set; }
    public Money Price { get; private set; }
}
