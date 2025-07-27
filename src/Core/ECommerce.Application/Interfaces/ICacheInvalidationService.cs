namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for cache invalidation operations
/// </summary>
public interface ICacheInvalidationService
{
    /// <summary>
    /// Invalidates product-related cache entries
    /// </summary>
    /// <param name="productId">Specific product ID to invalidate</param>
    /// <param name="categoryId">Category ID to invalidate related products</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateProductCacheAsync(Guid? productId = null, Guid? categoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates order-related cache entries
    /// </summary>
    /// <param name="orderId">Specific order ID to invalidate</param>
    /// <param name="customerId">Customer ID to invalidate related orders</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateOrderCacheAsync(Guid? orderId = null, Guid? customerId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates customer-related cache entries
    /// </summary>
    /// <param name="customerId">Specific customer ID to invalidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateCustomerCacheAsync(Guid? customerId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cache entries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateAllCacheAsync(CancellationToken cancellationToken = default);
}