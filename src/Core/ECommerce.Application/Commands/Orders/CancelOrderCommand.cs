namespace ECommerce.Application.Commands.Orders;

/// <summary>
/// Command to cancel an order
/// </summary>
public record CancelOrderCommand(
    Guid OrderId,
    string Reason
) : IRequest<Unit>;