using ECommerce.Domain.Aggregates.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the OrderItem entity with performance optimizations
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        // Primary key
        builder.HasKey(oi => oi.Id);

        // Properties from Entity base class
        builder.Property(oi => oi.Id)
            .ValueGeneratedNever();

        builder.Property(oi => oi.CreatedAt)
            .IsRequired();

        builder.Property(oi => oi.UpdatedAt)
            .IsRequired();

        // OrderItem properties
        builder.Property(oi => oi.OrderId)
            .IsRequired();

        builder.Property(oi => oi.ProductId)
            .IsRequired();

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(255);

        // ProductSku is not part of the OrderItem entity

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        // Configure Money value object for UnitPrice
        builder.OwnsOne(oi => oi.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("UnitPrice_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Configure Money value object for Discount
        builder.OwnsOne(oi => oi.Discount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Discount_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Discount_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Relationships
        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance Optimized Indexes
        
        // Index for order items lookup
        builder.HasIndex(oi => new { oi.OrderId, oi.ProductId });

        // Index for product sales analysis
        builder.HasIndex(oi => new { oi.ProductId, oi.CreatedAt });

        // Index for order totals calculation
        builder.HasIndex(oi => oi.OrderId);

        // Index for product popularity queries
        builder.HasIndex(oi => oi.ProductId);

        // Ignore calculated properties
        builder.Ignore(oi => oi.TotalPrice);
    }
}