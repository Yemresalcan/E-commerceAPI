namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for caching service operations
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key
    /// </summary>
    /// <typeparam name="T">Type of the cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached value or null if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in cache with expiration
    /// </summary>
    /// <typeparam name="T">Type of the value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiration">Cache expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in cache with default expiration
    /// </summary>
    /// <typeparam name="T">Type of the value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple values from cache by pattern
    /// </summary>
    /// <param name="pattern">Key pattern to match</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if key exists, false otherwise</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets a value in cache using a factory function
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to create value if not in cache</param>
    /// <param name="expiration">Cache expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached or newly created value</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;
}