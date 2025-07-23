namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when an order is delivered
/// </summary>
public record OrderDeliveredEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the delivered order
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// The unique identifier of the customer who placed the order
    /// </summary>
    public Guid CustomerId { get; init; }

    public OrderDeliveredEvent(
        Guid orderId,
        Guid customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
    }
}