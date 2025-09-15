using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;

namespace VerticalSliceArchitecture.Infrastructure
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
        }
        public DbSet<Domain.Product> Products => Set<Domain.Product>();
        public DbSet<Domain.Category> Categories => Set<Domain.Category>();
        public DbSet<Domain.FeatureFlag> FeatureFlags => Set<Domain.FeatureFlag>();
    }
}
