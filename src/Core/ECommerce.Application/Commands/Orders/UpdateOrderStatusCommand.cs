using ECommerce.Domain.Aggregates.OrderAggregate;

namespace ECommerce.Application.Commands.Orders;

/// <summary>
/// Command to update order status
/// </summary>
public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus NewStatus,
    string? Reason = null
) : IRequest<Unit>;