using ECommerce.Application.Interfaces;

namespace ECommerce.Infrastructure.Caching;

/// <summary>
/// Service for handling cache invalidation strategies
/// </summary>
public class CacheInvalidationService(
    ICacheService cacheService,
    ILogger<CacheInvalidationService> logger) : ICacheInvalidationService
{
    /// <summary>
    /// Invalidates product-related cache entries
    /// </summary>
    public async Task InvalidateProductCacheAsync(Guid? productId = null, Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Invalidating product cache for ProductId: {ProductId}, CategoryId: {CategoryId}", 
                productId, categoryId);

            // Invalidate specific product if provided
            if (productId.HasValue)
            {
                var productKey = CacheKeyGenerator.Product(productId.Value);
                await cacheService.RemoveAsync(productKey, cancellationToken);
            }

            // Invalidate all product lists and searches
            var productsPattern = CacheKeyGenerator.ProductsPattern();
            await cacheService.RemoveByPatternAsync(productsPattern, cancellationToken);

            logger.LogInformation("Successfully invalidated product cache");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating product cache");
        }
    }

    /// <summary>
    /// Invalidates order-related cache entries
    /// </summary>
    public async Task InvalidateOrderCacheAsync(Guid? orderId = null, Guid? customerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Invalidating order cache for OrderId: {OrderId}, CustomerId: {CustomerId}", 
                orderId, customerId);

            // Invalidate specific order if provided
            if (orderId.HasValue)
            {
                var orderKey = CacheKeyGenerator.Order(orderId.Value);
                await cacheService.RemoveAsync(orderKey, cancellationToken);
            }

            // Invalidate order lists
            var ordersPattern = CacheKeyGenerator.OrdersPattern(customerId);
            await cacheService.RemoveByPatternAsync(ordersPattern, cancellationToken);

            logger.LogInformation("Successfully invalidated order cache");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating order cache");
        }
    }

    /// <summary>
    /// Invalidates customer-related cache entries
    /// </summary>
    public async Task InvalidateCustomerCacheAsync(Guid? customerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Invalidating customer cache for CustomerId: {CustomerId}", customerId);

            // Invalidate specific customer if provided
            if (customerId.HasValue)
            {
                var customerKey = CacheKeyGenerator.Customer(customerId.Value);
                await cacheService.RemoveAsync(customerKey, cancellationToken);
            }

            // Invalidate all customer lists
            var customersPattern = CacheKeyGenerator.CustomersPattern();
            await cacheService.RemoveByPatternAsync(customersPattern, cancellationToken);

            logger.LogInformation("Successfully invalidated customer cache");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating customer cache");
        }
    }

    /// <summary>
    /// Invalidates all cache entries
    /// </summary>
    public async Task InvalidateAllCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Invalidating all cache entries");

            await cacheService.RemoveByPatternAsync("*", cancellationToken);

            logger.LogInformation("Successfully invalidated all cache entries");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating all cache entries");
        }
    }
}