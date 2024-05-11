using Domain.Entities.Customers;
using Domain.Entities.Products;

namespace Domain.Entities.Orders;

public class Order
{
    private readonly HashSet<LineItem> _lineItems = new();
    public OrderId? Id { get; private set; }
    public CustomerId? CustomerId { get; private set; }
    public IReadOnlyList<LineItem> LineItems => [.. _lineItems];
    public static Order Create(Customer customer)
    {
        var order = new Order()
        {
            Id = new OrderId(Guid.NewGuid()),
            CustomerId = customer.Id,
        };

        return order;
    }

    public void Add(Product product)
    {
        var lineItem = new LineItem(
            new LineItemId(Guid.NewGuid()),
            Id!,
            product.Id,
            product.Price!);
        _lineItems.Add(lineItem);
    }

    public bool RemoveLineItem(LineItemId lineItemId)
    {
        var lineItem = _lineItems.FirstOrDefault(x => x.Id == lineItemId);

        if (lineItem is null)
        {
            return false;
        }

        _lineItems.Remove(lineItem);

        return true;
    }
}
