using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Orders;

namespace ECommerce.Application.Handlers.Orders;

/// <summary>
/// Handler for GetOrderQuery
/// </summary>
public class GetOrderQueryHandler(
    IOrderQueryService orderQueryService,
    ILogger<GetOrderQueryHandler> logger)
    : IRequestHandler<GetOrderQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetOrderQuery for order ID: {OrderId}", request.OrderId);

        try
        {
            var orderDto = await orderQueryService.GetOrderByIdAsync(request.OrderId, cancellationToken);
            
            if (orderDto == null)
            {
                logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
                return null;
            }

            logger.LogInformation("Successfully retrieved order {OrderId}", request.OrderId);

            return orderDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while handling GetOrderQuery for order ID: {OrderId}", request.OrderId);
            throw;
        }
    }
}