namespace Domain.Entities.Orders;

public record OrderId
{
    public OrderId(long Value)
    {
        this.Value = Value;
    }

    public long Value { get; init; }

    public void Deconstruct(out long Value)
    {
        Value = this.Value;
    }
}
