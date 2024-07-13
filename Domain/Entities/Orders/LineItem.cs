using Domain.Core.Abstractions;

namespace Domain.Entities.Orders;

public class LineItem : IEntityBase<LineItemId>
{
    public LineItem(LineItemId id, OrderId orderId, Guid productId, Money price)
    {
        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Price = price;
    }

    public LineItem()
    {
    }

    public LineItemId Id { get; private set; }
    public OrderId OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Money Price { get; private set; }
}
