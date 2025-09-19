using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
        public DbSet<User> Users => Set<User>();

    }
}
