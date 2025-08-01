using ECommerce.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the ProductReview entity with performance optimizations
/// </summary>
public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");

        // Primary key
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(pr => pr.ProductId)
            .IsRequired();

        builder.Property(pr => pr.CustomerId)
            .IsRequired();

        builder.Property(pr => pr.Rating)
            .IsRequired();

        builder.Property(pr => pr.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pr => pr.Content)
            .HasMaxLength(2000);

        builder.Property(pr => pr.IsVerified)
            .IsRequired();

        builder.Property(pr => pr.IsApproved)
            .IsRequired();

        builder.Property(pr => pr.HelpfulVotes)
            .IsRequired();

        builder.Property(pr => pr.CreatedAt)
            .IsRequired();

        // Performance Optimized Indexes
        
        // Composite index for product reviews (most common query)
        builder.HasIndex(pr => new { pr.ProductId, pr.IsApproved, pr.CreatedAt })
            .HasDatabaseName("IX_ProductReviews_ProductId_IsApproved_CreatedAt")
            .HasFilter("IsApproved = true")
            .IsDescending(false, false, true); // ProductId ASC, IsApproved ASC, CreatedAt DESC

        // Index for customer reviews
        builder.HasIndex(pr => new { pr.CustomerId, pr.CreatedAt })
            .HasDatabaseName("IX_ProductReviews_CustomerId_CreatedAt")
            .IsDescending(false, true); // CustomerId ASC, CreatedAt DESC

        // Index for rating analysis
        builder.HasIndex(pr => new { pr.ProductId, pr.Rating, pr.IsApproved })
            .HasDatabaseName("IX_ProductReviews_ProductId_Rating_IsApproved")
            .HasFilter("IsApproved = true");

        // Index for verified purchase reviews
        builder.HasIndex(pr => new { pr.ProductId, pr.IsVerified, pr.IsApproved })
            .HasDatabaseName("IX_ProductReviews_ProductId_IsVerified_IsApproved")
            .HasFilter("IsVerified = true AND IsApproved = true");

        // Unique constraint to prevent duplicate reviews from same customer for same product
        builder.HasIndex(pr => new { pr.ProductId, pr.CustomerId })
            .IsUnique()
            .HasDatabaseName("IX_ProductReviews_ProductId_CustomerId_Unique");

        // Index for review moderation
        builder.HasIndex(pr => new { pr.IsApproved, pr.CreatedAt })
            .HasDatabaseName("IX_ProductReviews_Moderation")
            .HasFilter("IsApproved = false")
            .IsDescending(false, false); // Oldest first for moderation queue
    }
}