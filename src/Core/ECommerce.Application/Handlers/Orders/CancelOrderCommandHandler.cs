using ECommerce.Application.Commands.Orders;
using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Orders;

/// <summary>
/// Handler for CancelOrderCommand
/// </summary>
public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<CancelOrderCommandHandler> logger
) : IRequestHandler<CancelOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling order: {OrderId} with reason: {Reason}", 
            request.OrderId, request.Reason);

        // Get the order
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID '{request.OrderId}' not found");
        }

        // Check if we need to restore stock before cancelling
        var shouldRestoreStock = order.Status == OrderStatus.Pending || order.Status == OrderStatus.Confirmed;

        // Cancel the order
        order.Cancel(request.Reason);

        // Restore product stock quantities if order was not yet shipped
        if (shouldRestoreStock)
        {
            foreach (var orderItem in order.OrderItems)
            {
                var product = await productRepository.GetByIdAsync(orderItem.ProductId, cancellationToken);
                if (product != null)
                {
                    product.SetStock(product.StockQuantity + orderItem.Quantity, "Order cancelled - stock restored");
                    productRepository.Update(product);
                }
            }
        }

        // Update in repository
        orderRepository.Update(order);

        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully cancelled order {OrderId}", request.OrderId);

        return Unit.Value;
    }
}