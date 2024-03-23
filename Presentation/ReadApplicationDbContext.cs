using Microsoft.EntityFrameworkCore;

namespace Presentation
{
    public class ReadApplicationDbContext : DbContext
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
