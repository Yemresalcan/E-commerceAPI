namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when a product is updated
/// </summary>
public record ProductUpdatedEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the updated product
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// The updated name of the product
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The updated price amount of the product
    /// </summary>
    public decimal PriceAmount { get; init; }

    /// <summary>
    /// The currency of the product price
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    public ProductUpdatedEvent(
        Guid productId,
        string name,
        decimal priceAmount,
        string currency)
    {
        ProductId = productId;
        Name = name;
        PriceAmount = priceAmount;
        Currency = currency;
    }
}