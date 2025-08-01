using ECommerce.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Category entity with performance optimizations
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        // Primary key
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.ParentCategoryId);

        builder.Property(c => c.IsActive)
            .IsRequired();

        // Self-referencing relationship for hierarchical categories
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Performance Optimized Indexes
        
        // Index for active categories
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Categories_IsActive")
            .HasFilter("IsActive = true");

        // Index for hierarchical queries
        builder.HasIndex(c => new { c.ParentCategoryId, c.IsActive })
            .HasDatabaseName("IX_Categories_ParentId_IsActive")
            .HasFilter("IsActive = true");

        // Index for category name search
        builder.HasIndex(c => c.Name)
            .HasDatabaseName("IX_Categories_Name")
            .HasFilter("IsActive = true");

        // Index for root categories
        builder.HasIndex(c => c.ParentCategoryId)
            .HasDatabaseName("IX_Categories_RootCategories")
            .HasFilter("ParentCategoryId IS NULL AND IsActive = true");
    }
}