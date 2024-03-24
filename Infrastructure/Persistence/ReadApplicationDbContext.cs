using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class ReadApplicationDbContext : DbContext, IReadApplicationDbContext
    {
        public ReadApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReadApplicationDbContext).Assembly);
        }
    }
}
