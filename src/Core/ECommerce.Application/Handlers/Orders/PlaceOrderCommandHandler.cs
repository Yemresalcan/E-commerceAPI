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
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CustomerId"] = request.CustomerId,
            ["OrderItemCount"] = request.OrderItems.Count(),
            ["TotalOrderValue"] = request.OrderItems.Sum(x => x.UnitPrice * x.Quantity)
        })!)
        {
            logger.LogInformation("Starting order placement for customer {CustomerId} with {OrderItemCount} items", 
                request.CustomerId, request.OrderItems.Count());

            // Validate customer exists
            logger.LogDebug("Validating customer {CustomerId} exists", request.CustomerId);
            var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                logger.LogWarning("Order placement failed: Customer {CustomerId} not found", request.CustomerId);
                throw new InvalidOperationException($"Customer with ID '{request.CustomerId}' not found");
            }

            // Validate all products exist and create order items
            logger.LogDebug("Validating products and creating order items");
            var orderItems = new List<OrderItem>();
            foreach (var itemDto in request.OrderItems)
            {
                logger.LogDebug("Processing order item for product {ProductId}, quantity {Quantity}", 
                    itemDto.ProductId, itemDto.Quantity);

                var product = await productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);
                if (product == null)
                {
                    logger.LogWarning("Order placement failed: Product {ProductId} not found", itemDto.ProductId);
                    throw new InvalidOperationException($"Product with ID '{itemDto.ProductId}' not found");
                }

                // Validate stock availability
                if (product.StockQuantity < itemDto.Quantity)
                {
                    logger.LogWarning("Order placement failed: Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}", 
                        itemDto.ProductId, product.StockQuantity, itemDto.Quantity);
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
                logger.LogDebug("Added order item for product {ProductId} with quantity {Quantity}", 
                    itemDto.ProductId, itemDto.Quantity);
            }

            // Create the order
            logger.LogDebug("Creating order entity");
            var order = Order.Create(
                request.CustomerId,
                request.ShippingAddress,
                request.BillingAddress,
                orderItems
            );

            // Add to repository
            logger.LogDebug("Adding order {OrderId} to repository", order.Id);
            await orderRepository.AddAsync(order, cancellationToken);

            // Update product stock quantities
            logger.LogDebug("Updating product stock quantities");
            foreach (var itemDto in request.OrderItems)
            {
                var product = await productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);
                if (product != null)
                {
                    var newStock = product.StockQuantity - itemDto.Quantity;
                    product.SetStock(newStock, "Order placed");
                    productRepository.Update(product);
                    logger.LogDebug("Updated stock for product {ProductId} from {OldStock} to {NewStock}", 
                        itemDto.ProductId, product.StockQuantity + itemDto.Quantity, newStock);
                }
            }

            // Save changes
            logger.LogDebug("Saving changes for order {OrderId}", order.Id);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully placed order {OrderId} for customer {CustomerId} with total value {TotalValue}", 
                order.Id, request.CustomerId, order.TotalAmount.Amount);

            return order.Id;
        }
    }
}