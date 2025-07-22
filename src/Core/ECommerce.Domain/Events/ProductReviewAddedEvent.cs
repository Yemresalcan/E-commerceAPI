namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when a review is added to a product
/// </summary>
public record ProductReviewAddedEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the product
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// The unique identifier of the review
    /// </summary>
    public Guid ReviewId { get; init; }

    /// <summary>
    /// The customer who wrote the review
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// The rating given in the review
    /// </summary>
    public int Rating { get; init; }

    /// <summary>
    /// Whether the review is from a verified purchase
    /// </summary>
    public bool IsVerified { get; init; }

    public ProductReviewAddedEvent(
        Guid productId,
        Guid reviewId,
        Guid customerId,
        int rating,
        bool isVerified)
    {
        ProductId = productId;
        ReviewId = reviewId;
        CustomerId = customerId;
        Rating = rating;
        IsVerified = isVerified;
    }
}