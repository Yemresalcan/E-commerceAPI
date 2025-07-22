namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when a new product is created in the system
/// </summary>
public record ProductCreatedEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the created product
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// The name of the created product
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The price amount of the product
    /// </summary>
    public decimal PriceAmount { get; init; }

    /// <summary>
    /// The currency of the product price
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The category identifier the product belongs to
    /// </summary>
    public Guid CategoryId { get; init; }

    /// <summary>
    /// The initial stock quantity of the product
    /// </summary>
    public int StockQuantity { get; init; }

    public ProductCreatedEvent(
        Guid productId,
        string name,
        decimal priceAmount,
        string currency,
        Guid categoryId,
        int stockQuantity)
    {
        ProductId = productId;
        Name = name;
        PriceAmount = priceAmount;
        Currency = currency;
        CategoryId = categoryId;
        StockQuantity = stockQuantity;
    }
}