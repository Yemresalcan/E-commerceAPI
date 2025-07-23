using ECommerce.Domain.Aggregates.OrderAggregate;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for Order aggregate operations
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Gets orders by customer identifier
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of orders for the specified customer</returns>
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by status
    /// </summary>
    /// <param name="status">The order status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of orders with the specified status</returns>
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by customer and status
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <param name="status">The order status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of orders for the customer with the specified status</returns>
    Task<IEnumerable<Order>> GetByCustomerAndStatusAsync(
        Guid customerId, 
        OrderStatus status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders within a date range
    /// </summary>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of orders within the specified date range</returns>
    Task<IEnumerable<Order>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders that contain a specific product
    /// </summary>
    /// <param name="productId">The product identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of orders containing the specified product</returns>
    Task<IEnumerable<Order>> GetOrdersContainingProductAsync(
        Guid productId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending orders (orders that need processing)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pending orders</returns>
    Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders ready for shipping (confirmed orders)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of orders ready for shipping</returns>
    Task<IEnumerable<Order>> GetOrdersReadyForShippingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets shipped orders that haven't been delivered yet
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of shipped orders</returns>
    Task<IEnumerable<Order>> GetShippedOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders with pagination support
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of orders</returns>
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customer orders with pagination support
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of customer orders</returns>
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetCustomerOrdersPagedAsync(
        Guid customerId,
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent order for a customer
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The most recent order for the customer, or null if no orders exist</returns>
    Task<Order?> GetMostRecentOrderByCustomerAsync(
        Guid customerId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets order statistics for a customer
    /// </summary>
    /// <param name="customerId">The customer identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order statistics including total orders, total amount, etc.</returns>
    Task<(int TotalOrders, decimal TotalAmount, DateTime? LastOrderDate)> GetCustomerOrderStatsAsync(
        Guid customerId, 
        CancellationToken cancellationToken = default);
}