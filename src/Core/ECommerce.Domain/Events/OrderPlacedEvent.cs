namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when a new order is placed in the system
/// </summary>
public record OrderPlacedEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the placed order
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// The unique identifier of the customer who placed the order
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// The total amount of the order
    /// </summary>
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// The currency of the order total
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The number of items in the order
    /// </summary>
    public int ItemCount { get; init; }

    /// <summary>
    /// The shipping address for the order
    /// </summary>
    public string ShippingAddress { get; init; } = string.Empty;

    public OrderPlacedEvent(
        Guid orderId,
        Guid customerId,
        decimal totalAmount,
        string currency,
        int itemCount,
        string shippingAddress)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Currency = currency;
        ItemCount = itemCount;
        ShippingAddress = shippingAddress;
    }
}