namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when an order is confirmed
/// </summary>
public record OrderConfirmedEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the confirmed order
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

    public OrderConfirmedEvent(
        Guid orderId,
        Guid customerId,
        decimal totalAmount,
        string currency)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Currency = currency;
    }
}