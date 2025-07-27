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
}