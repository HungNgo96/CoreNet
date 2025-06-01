using Domain.Core.Abstractions;
using Domain.Entities.Products;

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

    public LineItem(Money price)
    {
        Price = price;
    }

    public long OrderId { get; private set; }
    public long ProductId { get; private set; }
    public Money Price { get; private set; }
}
