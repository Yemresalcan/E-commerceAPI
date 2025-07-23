namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when an order is shipped
/// </summary>
public record OrderShippedEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the shipped order
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// The unique identifier of the customer who placed the order
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// The shipping address for the order
    /// </summary>
    public string ShippingAddress { get; init; } = string.Empty;

    public OrderShippedEvent(
        Guid orderId,
        Guid customerId,
        string shippingAddress)
    {
        OrderId = orderId;
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
    }
}