namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when product stock is updated
/// </summary>
public record ProductStockUpdatedEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the product
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// The previous stock quantity
    /// </summary>
    public int PreviousStock { get; init; }

    /// <summary>
    /// The new stock quantity
    /// </summary>
    public int NewStock { get; init; }

    /// <summary>
    /// The reason for the stock change
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    public ProductStockUpdatedEvent(
        Guid productId,
        int previousStock,
        int newStock,
        string reason)
    {
        ProductId = productId;
        PreviousStock = previousStock;
        NewStock = newStock;
        Reason = reason;
    }
}