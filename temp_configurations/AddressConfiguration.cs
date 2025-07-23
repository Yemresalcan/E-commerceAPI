using ECommerce.Domain.Aggregates.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Address entity
/// </summary>
public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        // Primary key
        builder.HasKey(a => a.Id);

        // Properties from Entity base class
        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        // Address properties
        builder.Property(a => a.CustomerId)
            .IsRequired();

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.Street1)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Street2)
            .HasMaxLength(100);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.State)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.IsPrimary)
            .IsRequired();

        builder.Property(a => a.Label)
            .HasMaxLength(50);

        // Relationships
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.CustomerId);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => a.IsPrimary);
        builder.HasIndex(a => new { a.CustomerId, a.IsPrimary });
    }
}