using ECommerce.Domain.Aggregates.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Profile entity
/// </summary>
public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties from Entity base class
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Profile properties
        builder.Property(p => p.CustomerId)
            .IsRequired();

        builder.Property(p => p.DateOfBirth);

        builder.Property(p => p.Gender)
            .HasMaxLength(20);

        builder.Property(p => p.PreferredLanguage)
            .HasMaxLength(10);

        builder.Property(p => p.PreferredCurrency)
            .HasMaxLength(3);

        builder.Property(p => p.NewsletterSubscribed)
            .IsRequired();

        builder.Property(p => p.MarketingEmailsEnabled)
            .IsRequired();

        builder.Property(p => p.SmsNotificationsEnabled)
            .IsRequired();

        // Relationships
        builder.HasOne<Customer>()
            .WithOne()
            .HasForeignKey<Profile>(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.CustomerId)
            .IsUnique();

        builder.HasIndex(p => p.PreferredLanguage);
        builder.HasIndex(p => p.PreferredCurrency);
        builder.HasIndex(p => p.NewsletterSubscribed);
    }
}