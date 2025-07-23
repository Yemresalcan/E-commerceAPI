namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when an order is cancelled
/// </summary>
public record OrderCancelledEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the cancelled order
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// The unique identifier of the customer who placed the order
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// The reason for cancelling the order
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    public OrderCancelledEvent(
        Guid orderId,
        Guid customerId,
        string reason)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Reason = reason;
    }
}