using ECommerce.Application.Commands.Orders;
using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.Handlers.Orders;

/// <summary>
/// Handler for PlaceOrderCommand
/// </summary>
public class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<PlaceOrderCommandHandler> logger
) : IRequestHandler<PlaceOrderCommand, Guid>
{
    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Placing order for customer: {CustomerId}", request.CustomerId);

        // Validate customer exists
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID '{request.CustomerId}' not found");
        }

        // Validate all products exist and create order items
        var orderItems = new List<OrderItem>();
        foreach (var itemDto in request.OrderItems)
        {
            var product = await productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID '{itemDto.ProductId}' not found");
            }

            // Validate stock availability
            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {itemDto.Quantity}");
            }

            var unitPrice = new Money(itemDto.UnitPrice, itemDto.Currency);
            var discount = itemDto.Discount > 0 ? new Money(itemDto.Discount, itemDto.Currency) : null;

            var orderItem = OrderItem.Create(
                itemDto.ProductId,
                itemDto.ProductName,
                itemDto.Quantity,
                unitPrice,
                discount
            );

            orderItems.Add(orderItem);
        }

        // Create the order
        var order = Order.Create(
            request.CustomerId,
            request.ShippingAddress,
            request.BillingAddress,
            orderItems
        );

        // Add to repository
        await orderRepository.AddAsync(order, cancellationToken);

        // Update product stock quantities
        foreach (var itemDto in request.OrderItems)
        {
            var product = await productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);
            if (product != null)
            {
                product.SetStock(product.StockQuantity - itemDto.Quantity, "Order placed");
                productRepository.Update(product);
            }
        }

        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully placed order with ID: {OrderId}", order.Id);

        return order.Id;
    }
}