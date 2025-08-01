using ECommerce.Domain.Aggregates.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Order entity with performance optimizations
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        // Primary key
        builder.HasKey(o => o.Id);

        // Properties from AggregateRoot
        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.Version)
            .IsConcurrencyToken();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        // Order properties
        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.ShippingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.BillingAddress)
            .IsRequired()
            .HasMaxLength(500);

        // Configure relationship with OrderItems
        builder.HasMany<OrderItem>()
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with Payment (one-to-one)
        builder.HasOne(o => o.Payment)
            .WithOne()
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance Optimized Indexes
        
        // Index for customer orders (most common query)
        builder.HasIndex(o => new { o.CustomerId, o.CreatedAt });

        // Index for order status queries
        builder.HasIndex(o => new { o.Status, o.CreatedAt });

        // Index for customer and status queries
        builder.HasIndex(o => new { o.CustomerId, o.Status });

        // Index for date range queries (reporting)
        builder.HasIndex(o => o.CreatedAt);

        // Index for order updates (for cache invalidation)
        builder.HasIndex(o => o.UpdatedAt);

        // Ignore domain events and calculated properties
        builder.Ignore(o => o.DomainEvents);
        builder.Ignore(o => o.OrderItems);
        builder.Ignore(o => o.TotalAmount);
        builder.Ignore(o => o.TotalItemCount);
    }
}