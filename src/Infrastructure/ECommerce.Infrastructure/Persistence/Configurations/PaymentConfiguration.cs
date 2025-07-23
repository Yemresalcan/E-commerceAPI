using ECommerce.Domain.Aggregates.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Payment entity
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties from Entity base class
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Payment properties
        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.TransactionId)
            .HasMaxLength(100);

        builder.Property(p => p.ProcessedAt);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.OrderId)
            .IsUnique();

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.TransactionId);
        builder.HasIndex(p => p.ProcessedAt);
    }
}