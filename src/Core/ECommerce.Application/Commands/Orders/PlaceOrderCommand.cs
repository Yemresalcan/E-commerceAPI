using ECommerce.Application.DTOs;

namespace ECommerce.Application.Commands.Orders;

/// <summary>
/// Command to place a new order
/// </summary>
public record PlaceOrderCommand(
    Guid CustomerId,
    string ShippingAddress,
    string BillingAddress,
    IEnumerable<OrderItemDto> OrderItems
) : IRequest<Guid>;