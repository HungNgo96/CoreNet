using Domain.Entities;
using Domain.Entities.Customers;
using Domain.Entities.Orders;
using Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace Domain.Core.SharedKernel
{
    public interface IReadApplicationDbContext : IApplicationDbContext
    {
    }
}
