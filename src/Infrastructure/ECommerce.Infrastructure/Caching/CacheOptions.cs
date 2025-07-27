namespace ECommerce.Infrastructure.Caching;

/// <summary>
/// Configuration options for caching
/// </summary>
public class CacheOptions
{
    public const string SectionName = "Cache";

    /// <summary>
    /// Default cache expiration time in minutes
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Product cache expiration time in minutes
    /// </summary>
    public int ProductCacheExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Order cache expiration time in minutes
    /// </summary>
    public int OrderCacheExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Customer cache expiration time in minutes
    /// </summary>
    public int CustomerCacheExpirationMinutes { get; set; } = 45;

    /// <summary>
    /// Search results cache expiration time in minutes
    /// </summary>
    public int SearchCacheExpirationMinutes { get; set; } = 10;

    /// <summary>
    /// Whether caching is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Cache key prefix
    /// </summary>
    public string KeyPrefix { get; set; } = "ecommerce";
}