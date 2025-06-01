using Domain.Core;
using Domain.Core.Abstractions;
using Domain.Entities.Customers;
using Domain.Entities.Products;

namespace Domain.Entities.Orders;

public class Order : EntityBase
{
    private readonly HashSet<LineItem> _lineItems = new();
    public long? CustomerId { get; private set; }
    public IReadOnlyList<LineItem> LineItems => [.. _lineItems];

    public static Order Create(Customer customer)
    {
        var order = new Order()
        {
            Id = NumericIdGenerator.Generate(),
            CustomerId = customer.Id,
        };

        return order;
    }

    public void Add(Product product)
    {
        var lineItem = new LineItem(
            NumericIdGenerator.Generate(),
            Id!,
            product.Id,
            product.Price!);
        _lineItems.Add(lineItem);
    }

    public bool RemoveLineItem(long lineItemId)
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
