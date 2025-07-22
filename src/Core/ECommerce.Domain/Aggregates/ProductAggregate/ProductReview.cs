using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Aggregates.ProductAggregate;

/// <summary>
/// Represents a customer review for a product
/// </summary>
public class ProductReview : Entity
{
    /// <summary>
    /// The product this review belongs to
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// The customer who wrote this review
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// The rating given by the customer (1-5 stars)
    /// </summary>
    public int Rating { get; private set; }

    /// <summary>
    /// The review title
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// The review content/comment
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Whether this review is verified (customer actually purchased the product)
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Whether this review is approved and visible
    /// </summary>
    public bool IsApproved { get; private set; }

    /// <summary>
    /// Number of helpful votes this review received
    /// </summary>
    public int HelpfulVotes { get; private set; }

    // Private constructor for EF Core
    private ProductReview() : base()
    {
        Title = string.Empty;
        Content = string.Empty;
    }

    /// <summary>
    /// Creates a new product review
    /// </summary>
    public static ProductReview Create(
        Guid productId,
        Guid customerId,
        int rating,
        string title,
        string content,
        bool isVerified = false)
    {
        ValidateRating(rating);
        ValidateTitle(title);
        ValidateContent(content);

        return new ProductReview
        {
            ProductId = productId,
            CustomerId = customerId,
            Rating = rating,
            Title = title,
            Content = content,
            IsVerified = isVerified,
            IsApproved = false, // Reviews need approval by default
            HelpfulVotes = 0
        };
    }

    /// <summary>
    /// Updates the review content and rating
    /// </summary>
    public void Update(int rating, string title, string content)
    {
        ValidateRating(rating);
        ValidateTitle(title);
        ValidateContent(content);

        Rating = rating;
        Title = title;
        Content = content;
        IsApproved = false; // Reset approval status when updated
        MarkAsModified();
    }

    /// <summary>
    /// Approves the review for public display
    /// </summary>
    public void Approve()
    {
        IsApproved = true;
        MarkAsModified();
    }

    /// <summary>
    /// Rejects/disapproves the review
    /// </summary>
    public void Reject()
    {
        IsApproved = false;
        MarkAsModified();
    }

    /// <summary>
    /// Marks the review as verified (customer purchased the product)
    /// </summary>
    public void MarkAsVerified()
    {
        IsVerified = true;
        MarkAsModified();
    }

    /// <summary>
    /// Adds a helpful vote to the review
    /// </summary>
    public void AddHelpfulVote()
    {
        HelpfulVotes++;
        MarkAsModified();
    }

    /// <summary>
    /// Removes a helpful vote from the review (if possible)
    /// </summary>
    public void RemoveHelpfulVote()
    {
        if (HelpfulVotes > 0)
        {
            HelpfulVotes--;
            MarkAsModified();
        }
    }

    private static void ValidateRating(int rating)
    {
        if (rating < 1 || rating > 5)
        {
            throw new InvalidRatingException(rating);
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Review title cannot be null or empty", nameof(title));
        }

        if (title.Length > 200)
        {
            throw new ArgumentException("Review title cannot exceed 200 characters", nameof(title));
        }
    }

    private static void ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidReviewContentException();
        }

        if (content.Length > 2000)
        {
            throw new ArgumentException("Review content cannot exceed 2000 characters", nameof(content));
        }
    }
}