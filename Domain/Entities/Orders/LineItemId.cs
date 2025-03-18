namespace Domain.Entities.Orders;

public record LineItemId
{
    public LineItemId(long Value)
    {
        this.Value = Value;
    }

    public long Value { get; init; }

    public void Deconstruct(out long Value)
    {
        Value = this.Value;
    }
}
