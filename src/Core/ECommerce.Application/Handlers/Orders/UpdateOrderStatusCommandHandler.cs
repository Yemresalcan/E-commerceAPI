using ECommerce.Application.Commands.Orders;
using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Orders;

/// <summary>
/// Handler for UpdateOrderStatusCommand
/// </summary>
public class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateOrderStatusCommandHandler> logger
) : IRequestHandler<UpdateOrderStatusCommand, Unit>
{
    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating order status for order: {OrderId} to {NewStatus}", 
            request.OrderId, request.NewStatus);

        // Get the order
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID '{request.OrderId}' not found");
        }

        // Update status based on the new status
        switch (request.NewStatus)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Shipped:
                order.Ship();
                break;
            case OrderStatus.Delivered:
                order.Deliver();
                break;
            case OrderStatus.Cancelled:
                var reason = request.Reason ?? "Order cancelled by system";
                order.Cancel(reason);
                break;
            case OrderStatus.Pending:
                throw new InvalidOperationException("Cannot change order status back to Pending");
            default:
                throw new InvalidOperationException($"Unsupported order status: {request.NewStatus}");
        }

        // Update in repository
        orderRepository.Update(order);

        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated order {OrderId} status to {NewStatus}", 
            request.OrderId, request.NewStatus);

        return Unit.Value;
    }
}