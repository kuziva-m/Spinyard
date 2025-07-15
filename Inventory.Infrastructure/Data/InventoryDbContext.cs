using Inventory.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Core.Domain.Entities.Attribute> Attributes { get; set; }
        public DbSet<AttributeOption> AttributeOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // This more explicit configuration for the many-to-many relationship
            // can resolve issues with data loading.
            modelBuilder.Entity<ProductVariant>()
                .HasMany(p => p.AttributeOptions)
                .WithMany() // Since there is no navigation property on the other side
                .UsingEntity<Dictionary<string, object>>(
                    "ProductVariantValues",
                    j => j
                        .HasOne<AttributeOption>()
                        .WithMany()
                        .HasForeignKey("AttributeOptionsId"),
                    j => j
                        .HasOne<ProductVariant>()
                        .WithMany()
                        .HasForeignKey("ProductVariantId"));

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}