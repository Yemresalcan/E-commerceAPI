using ECommerce.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Caching.Decorators;

/// <summary>
/// Generic caching decorator for query handlers
/// </summary>
/// <typeparam name="TQuery">Query type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class CachedQueryHandlerDecorator<TQuery, TResponse>(
    IRequestHandler<TQuery, TResponse> handler,
    ICacheService cacheService,
    IOptions<CacheOptions> cacheOptions,
    ILogger<CachedQueryHandlerDecorator<TQuery, TResponse>> logger)
    : IRequestHandler<TQuery, TResponse>
    where TQuery : IRequest<TResponse>
    where TResponse : class
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;

    public async Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken)
    {
        if (!_cacheOptions.Enabled)
        {
            return await handler.Handle(request, cancellationToken);
        }

        var cacheKey = GenerateCacheKey(request);
        var expiration = GetCacheExpiration(typeof(TQuery));

        logger.LogDebug("Attempting to retrieve cached result for query: {QueryType} with key: {CacheKey}", 
            typeof(TQuery).Name, cacheKey);

        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => handler.Handle(request, cancellationToken),
            expiration,
            cancellationToken);
    }

    private string GenerateCacheKey(TQuery request)
    {
        var queryName = typeof(TQuery).Name.Replace("Query", "").ToLowerInvariant();
        var requestHash = request.GetHashCode().ToString();
        return $"{_cacheOptions.KeyPrefix}:query:{queryName}:{requestHash}";
    }

    private TimeSpan GetCacheExpiration(Type queryType)
    {
        var queryName = queryType.Name.ToLowerInvariant();
        
        return queryName switch
        {
            var name when name.Contains("product") => TimeSpan.FromMinutes(_cacheOptions.ProductCacheExpirationMinutes),
            var name when name.Contains("order") => TimeSpan.FromMinutes(_cacheOptions.OrderCacheExpirationMinutes),
            var name when name.Contains("customer") => TimeSpan.FromMinutes(_cacheOptions.CustomerCacheExpirationMinutes),
            var name when name.Contains("search") => TimeSpan.FromMinutes(_cacheOptions.SearchCacheExpirationMinutes),
            _ => TimeSpan.FromMinutes(_cacheOptions.DefaultExpirationMinutes)
        };
    }
}