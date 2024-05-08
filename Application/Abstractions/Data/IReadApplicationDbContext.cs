using Domain.Entities;
using Domain.Entities.Customers;
using Domain.Entities.Orders;
using Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data
{
    public interface IReadApplicationDbContext : IApplicationDbContext
    {
    }
}
