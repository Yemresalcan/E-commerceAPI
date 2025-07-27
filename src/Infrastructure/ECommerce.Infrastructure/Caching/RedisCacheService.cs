using System.Text.Json;
using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ECommerce.Infrastructure.Caching;

/// <summary>
/// Redis implementation of the cache service
/// </summary>
public class RedisCacheService(
    IDistributedCache distributedCache,
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cached = await distributedCache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(cached))
            {
                logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cached);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving value from cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await distributedCache.SetStringAsync(key, serialized, options, cancellationToken);
            logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
    {
        await SetAsync(key, value, _defaultExpiration, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
            logger.LogDebug("Removed cache entry for key: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);
            
            var keyArray = keys.ToArray();
            if (keyArray.Length > 0)
            {
                await _database.KeyDeleteAsync(keyArray);
                logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keyArray.Length, pattern);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing values from cache by pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if key exists in cache: {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        logger.LogDebug("Cache miss for key: {Key}, executing factory function", key);
        var value = await factory();
        await SetAsync(key, value, expiration, cancellationToken);
        
        return value;
    }
}