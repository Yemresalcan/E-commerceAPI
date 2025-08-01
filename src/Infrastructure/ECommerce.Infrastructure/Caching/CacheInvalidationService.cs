using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;

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

    /// <summary>
    /// Handles cache invalidation based on domain events for performance optimization
    /// </summary>
    public async Task HandleDomainEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (domainEvent)
            {
                case ProductCreatedEvent productCreated:
                    await InvalidateProductCacheAsync(productCreated.ProductId, cancellationToken: cancellationToken);
                    await InvalidateSearchCacheAsync("product", cancellationToken);
                    break;

                case OrderPlacedEvent orderPlaced:
                    await InvalidateOrderCacheAsync(orderPlaced.OrderId, orderPlaced.CustomerId, cancellationToken);
                    await InvalidateCustomerCacheAsync(orderPlaced.CustomerId, cancellationToken);
                    break;

                case CustomerRegisteredEvent customerRegistered:
                    await InvalidateCustomerCacheAsync(customerRegistered.CustomerId, cancellationToken);
                    break;

                default:
                    logger.LogDebug("No specific cache invalidation strategy for event type: {EventType}", 
                        domainEvent.GetType().Name);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle cache invalidation for domain event: {EventType}", 
                domainEvent.GetType().Name);
        }
    }

    /// <summary>
    /// Invalidates search-related cache entries for performance optimization
    /// </summary>
    public async Task InvalidateSearchCacheAsync(string searchType = "*", CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Invalidating search cache for type: {SearchType}", searchType);

            var searchPattern = $"search:*{searchType}*";
            await cacheService.RemoveByPatternAsync(searchPattern, cancellationToken);

            // Also invalidate Elasticsearch cached results
            var elasticsearchPattern = $"elasticsearch:*{searchType}*";
            await cacheService.RemoveByPatternAsync(elasticsearchPattern, cancellationToken);

            logger.LogInformation("Successfully invalidated search cache for type: {SearchType}", searchType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating search cache for type: {SearchType}", searchType);
        }
    }

    /// <summary>
    /// Preloads frequently accessed data into cache for performance optimization
    /// </summary>
    public async Task PreloadFrequentDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting cache preload for frequently accessed data");

            // Preload featured products
            var featuredProductsKey = CacheKeyGenerator.FeaturedProducts();
            if (!await cacheService.ExistsAsync(featuredProductsKey, cancellationToken))
            {
                logger.LogDebug("Preloading featured products cache");
                // Implementation would fetch and cache featured products
            }

            // Preload popular categories
            var categoriesKey = CacheKeyGenerator.PopularCategories();
            if (!await cacheService.ExistsAsync(categoriesKey, cancellationToken))
            {
                logger.LogDebug("Preloading popular categories cache");
                // Implementation would fetch and cache popular categories
            }

            logger.LogInformation("Completed cache preload for frequently accessed data");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to preload frequently accessed data into cache");
        }
    }
}