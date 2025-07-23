using ECommerce.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Category entity
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties from Entity base class
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        // Category properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.ParentCategoryId);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.SortOrder)
            .IsRequired();

        // Self-referencing relationship for hierarchical categories
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.ParentCategoryId);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.SortOrder);

        // Ignore calculated properties
        builder.Ignore(c => c.IsRootCategory);
    }
}