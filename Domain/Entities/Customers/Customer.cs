using Domain.Core.Abstractions;

namespace Domain.Entities.Customers;

public class Customer : EntityBase
{
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
}
