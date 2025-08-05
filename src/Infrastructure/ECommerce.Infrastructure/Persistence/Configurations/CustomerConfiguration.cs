using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Customer entity with performance optimizations
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties from AggregateRoot
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Version)
            .IsConcurrencyToken();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        // Customer properties
        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.RegistrationDate)
            .IsRequired();

        builder.Property(c => c.LastActiveDate);

        // Configure Email value object
        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(255);
        });

        // Configure PhoneNumber value object (nullable)
        builder.OwnsOne(c => c.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20);
        });

        // Configure relationship with Profile (one-to-one)
        builder.HasOne(c => c.Profile)
            .WithOne()
            .HasForeignKey<Profile>("CustomerId")
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with Addresses (one-to-many)
        builder.HasMany(c => c.Addresses)
            .WithOne()
            .HasForeignKey("CustomerId")
            .OnDelete(DeleteBehavior.Cascade);

        // Performance Optimized Indexes
        
        // Note: Email index is configured in the owned type configuration

        // Index for active customers
        builder.HasIndex(c => new { c.IsActive, c.RegistrationDate });

        // Index for customer activity tracking
        builder.HasIndex(c => c.LastActiveDate);

        // Index for customer search by name
        builder.HasIndex(c => new { c.FirstName, c.LastName });

        // Index for registration date queries (reporting)
        builder.HasIndex(c => c.RegistrationDate);

        // Index for customer updates (for cache invalidation)
        builder.HasIndex(c => c.UpdatedAt);

        // Ignore domain events and calculated properties
        builder.Ignore(c => c.DomainEvents);
        builder.Ignore(c => c.PrimaryAddress);
        builder.Ignore(c => c.FullName);
        builder.Ignore(c => c.HasAddresses);
        builder.Ignore(c => c.HasPrimaryAddress);
    }
}