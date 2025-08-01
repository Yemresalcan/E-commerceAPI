using System.Text;

namespace ECommerce.Infrastructure.Caching;

/// <summary>
/// Utility class for generating cache keys
/// </summary>
public static class CacheKeyGenerator
{
    private const string Separator = ":";

    /// <summary>
    /// Generates a cache key for products list
    /// </summary>
    public static string ProductsList(int page, int pageSize, string? searchTerm = null, Guid? categoryId = null)
    {
        var keyBuilder = new StringBuilder($"products{Separator}list{Separator}{page}{Separator}{pageSize}");
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            keyBuilder.Append($"{Separator}search{Separator}{searchTerm.ToLowerInvariant()}");
        }
        
        if (categoryId.HasValue)
        {
            keyBuilder.Append($"{Separator}category{Separator}{categoryId.Value}");
        }
        
        return keyBuilder.ToString();
    }

    /// <summary>
    /// Generates a cache key for product search
    /// </summary>
    public static string ProductsSearch(string searchTerm, int page, int pageSize)
    {
        return $"products{Separator}search{Separator}{searchTerm.ToLowerInvariant()}{Separator}{page}{Separator}{pageSize}";
    }

    /// <summary>
    /// Generates a cache key for a single product
    /// </summary>
    public static string Product(Guid productId)
    {
        return $"product{Separator}{productId}";
    }

    /// <summary>
    /// Generates a cache key for orders list
    /// </summary>
    public static string OrdersList(Guid customerId, int page, int pageSize)
    {
        return $"orders{Separator}list{Separator}{customerId}{Separator}{page}{Separator}{pageSize}";
    }

    /// <summary>
    /// Generates a cache key for a single order
    /// </summary>
    public static string Order(Guid orderId)
    {
        return $"order{Separator}{orderId}";
    }

    /// <summary>
    /// Generates a cache key for customers list
    /// </summary>
    public static string CustomersList(int page, int pageSize, string? searchTerm = null)
    {
        var keyBuilder = new StringBuilder($"customers{Separator}list{Separator}{page}{Separator}{pageSize}");
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            keyBuilder.Append($"{Separator}search{Separator}{searchTerm.ToLowerInvariant()}");
        }
        
        return keyBuilder.ToString();
    }

    /// <summary>
    /// Generates a cache key for a single customer
    /// </summary>
    public static string Customer(Guid customerId)
    {
        return $"customer{Separator}{customerId}";
    }

    /// <summary>
    /// Generates a pattern for invalidating product-related cache entries
    /// </summary>
    public static string ProductsPattern()
    {
        return $"products{Separator}*";
    }

    /// <summary>
    /// Generates a pattern for invalidating order-related cache entries
    /// </summary>
    public static string OrdersPattern(Guid? customerId = null)
    {
        return customerId.HasValue 
            ? $"orders{Separator}*{Separator}{customerId.Value}{Separator}*"
            : $"orders{Separator}*";
    }

    /// <summary>
    /// Generates a pattern for invalidating customer-related cache entries
    /// </summary>
    public static string CustomersPattern()
    {
        return $"customers{Separator}*";
    }

    /// <summary>
    /// Generates a cache key for featured products (performance optimization)
    /// </summary>
    public static string FeaturedProducts(int count = 10)
    {
        return $"products{Separator}featured{Separator}{count}";
    }

    /// <summary>
    /// Generates a cache key for popular categories (performance optimization)
    /// </summary>
    public static string PopularCategories(int count = 20)
    {
        return $"categories{Separator}popular{Separator}{count}";
    }

    /// <summary>
    /// Generates a cache key for product by SKU (performance optimization)
    /// </summary>
    public static string ProductBySku(string sku)
    {
        return $"product{Separator}sku{Separator}{sku.ToLowerInvariant()}";
    }

    /// <summary>
    /// Generates a cache key for low stock products (performance optimization)
    /// </summary>
    public static string LowStockProducts(int page, int pageSize)
    {
        return $"products{Separator}lowstock{Separator}{page}{Separator}{pageSize}";
    }

    /// <summary>
    /// Generates a cache key for customer order statistics (performance optimization)
    /// </summary>
    public static string CustomerOrderStats(Guid customerId)
    {
        return $"customer{Separator}{customerId}{Separator}stats";
    }

    /// <summary>
    /// Generates a cache key for product reviews (performance optimization)
    /// </summary>
    public static string ProductReviews(Guid productId, int page, int pageSize)
    {
        return $"product{Separator}{productId}{Separator}reviews{Separator}{page}{Separator}{pageSize}";
    }

    /// <summary>
    /// Generates a cache key for category products count (performance optimization)
    /// </summary>
    public static string CategoryProductsCount(Guid categoryId)
    {
        return $"category{Separator}{categoryId}{Separator}count";
    }

    /// <summary>
    /// Generates a cache key for Elasticsearch search results (performance optimization)
    /// </summary>
    public static string ElasticsearchSearch(string indexName, string query, int page, int pageSize)
    {
        var queryHash = query.GetHashCode();
        return $"elasticsearch{Separator}{indexName}{Separator}{queryHash}{Separator}{page}{Separator}{pageSize}";
    }

    /// <summary>
    /// Generates a cache key for product suggestions (performance optimization)
    /// </summary>
    public static string ProductSuggestions(string query, int count = 10)
    {
        return $"suggestions{Separator}products{Separator}{query.ToLowerInvariant()}{Separator}{count}";
    }

    /// <summary>
    /// Generates a cache key for similar products (performance optimization)
    /// </summary>
    public static string SimilarProducts(Guid productId, int count = 5)
    {
        return $"product{Separator}{productId}{Separator}similar{Separator}{count}";
    }
}