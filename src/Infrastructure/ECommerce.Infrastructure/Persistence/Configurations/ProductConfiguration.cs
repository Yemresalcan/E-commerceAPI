using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Product entity with performance optimizations
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties from AggregateRoot
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Version)
            .IsConcurrencyToken();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Product properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.StockQuantity)
            .IsRequired();

        builder.Property(p => p.MinimumStockLevel)
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.IsFeatured)
            .IsRequired();

        builder.Property(p => p.Weight)
            .HasPrecision(18, 3);

        builder.Property(p => p.Dimensions)
            .HasMaxLength(100);

        // Configure Money value object
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Price_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Price_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Configure relationship with Category
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure relationship with ProductReviews
        builder.HasMany(p => p.Reviews)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        // Performance Optimized Indexes
        
        // Unique index for SKU (business requirement)
        builder.HasIndex(p => p.Sku)
            .IsUnique();

        // Index for active products by category (most common query)
        builder.HasIndex(p => new { p.CategoryId, p.IsActive });

        // Index for featured active products
        builder.HasIndex(p => new { p.IsFeatured, p.IsActive });

        // Index for stock management queries
        builder.HasIndex(p => new { p.StockQuantity, p.IsActive });

        // Index for product search by name
        builder.HasIndex(p => p.Name);

        // Index for recent products (ordering by creation date)
        builder.HasIndex(p => p.CreatedAt);

        // Index for product updates (for cache invalidation)
        builder.HasIndex(p => p.UpdatedAt);

        // Ignore domain events (they are not persisted)
        builder.Ignore(p => p.DomainEvents);

        // Ignore calculated properties
        builder.Ignore(p => p.AverageRating);
        builder.Ignore(p => p.ReviewCount);
        builder.Ignore(p => p.IsInStock);
        builder.Ignore(p => p.IsLowStock);
        builder.Ignore(p => p.IsOutOfStock);
    }
}