using ECommerce.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the ProductReview entity
/// </summary>
public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");

        // Primary key
        builder.HasKey(pr => pr.Id);

        // Properties from Entity base class
        builder.Property(pr => pr.Id)
            .ValueGeneratedNever();

        builder.Property(pr => pr.CreatedAt)
            .IsRequired();

        builder.Property(pr => pr.UpdatedAt)
            .IsRequired();

        // ProductReview properties
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
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(pr => pr.IsApproved)
            .IsRequired();

        builder.Property(pr => pr.IsVerified)
            .IsRequired();

        builder.Property(pr => pr.HelpfulCount)
            .IsRequired();

        // Relationships
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pr => pr.ProductId);
        builder.HasIndex(pr => pr.CustomerId);
        builder.HasIndex(pr => pr.Rating);
        builder.HasIndex(pr => pr.IsApproved);
        builder.HasIndex(pr => pr.CreatedAt);

        // Composite index for unique customer-product review
        builder.HasIndex(pr => new { pr.ProductId, pr.CustomerId })
            .IsUnique();
    }
}