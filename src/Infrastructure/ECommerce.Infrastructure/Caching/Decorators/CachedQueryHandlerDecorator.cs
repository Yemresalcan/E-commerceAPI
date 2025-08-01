using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Infrastructure.Caching.Decorators;

/// <summary>
/// Generic caching decorator for query handlers to improve performance
/// </summary>
/// <typeparam name="TRequest">The query request type</typeparam>
/// <typeparam name="TResponse">The query response type</typeparam>
public class CachedQueryHandlerDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ICacheableQuery
    where TResponse : class
{
    private readonly IRequestHandler<TRequest, TResponse> _handler;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedQueryHandlerDecorator<TRequest, TResponse>> _logger;

    public CachedQueryHandlerDecorator(
        IRequestHandler<TRequest, TResponse> handler,
        ICacheService cacheService,
        ILogger<CachedQueryHandlerDecorator<TRequest, TResponse>> logger)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        // Check if caching is enabled for this query
        if (!request.CacheEnabled)
        {
            _logger.LogDebug("Caching disabled for query {QueryType}", typeof(TRequest).Name);
            return await _handler.Handle(request, cancellationToken);
        }

        var cacheKey = request.GetCacheKey();
        
        // Try to get from cache first
        var cachedResult = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            _logger.LogDebug("Cache hit for query {QueryType} with key {CacheKey}", typeof(TRequest).Name, cacheKey);
            return cachedResult;
        }

        _logger.LogDebug("Cache miss for query {QueryType} with key {CacheKey}", typeof(TRequest).Name, cacheKey);

        // Execute the actual handler
        var result = await _handler.Handle(request, cancellationToken);

        // Cache the result if it's not null
        if (result != null)
        {
            await _cacheService.SetAsync(cacheKey, result, request.CacheDuration, cancellationToken);
            _logger.LogDebug("Cached result for query {QueryType} with key {CacheKey} for {Duration}", 
                typeof(TRequest).Name, cacheKey, request.CacheDuration);
        }

        return result!;
    }
}

/// <summary>
/// Interface for queries that support caching
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Gets whether caching is enabled for this query
    /// </summary>
    bool CacheEnabled { get; }

    /// <summary>
    /// Gets the cache duration for this query
    /// </summary>
    TimeSpan CacheDuration { get; }

    /// <summary>
    /// Gets the cache key for this query
    /// </summary>
    /// <returns>The cache key</returns>
    string GetCacheKey();
}

/// <summary>
/// Base class for cacheable queries
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public abstract class CacheableQuery<TResponse> : IRequest<TResponse>, ICacheableQuery
    where TResponse : class
{
    /// <summary>
    /// Gets whether caching is enabled for this query (default: true)
    /// </summary>
    public virtual bool CacheEnabled => true;

    /// <summary>
    /// Gets the cache duration for this query (default: 30 minutes)
    /// </summary>
    public virtual TimeSpan CacheDuration => TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets the cache key for this query
    /// </summary>
    /// <returns>The cache key</returns>
    public abstract string GetCacheKey();
}

/// <summary>
/// Cached product search query
/// </summary>
public class CachedProductSearchQuery : CacheableQuery<IEnumerable<object>>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CategoryId { get; set; }

    public override TimeSpan CacheDuration => TimeSpan.FromMinutes(15);

    public override string GetCacheKey()
    {
        return CacheKeyGenerator.ProductsList(Page, PageSize, SearchTerm, CategoryId);
    }
}

/// <summary>
/// Cached customer orders query
/// </summary>
public class CachedCustomerOrdersQuery : CacheableQuery<IEnumerable<object>>
{
    public Guid CustomerId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public override TimeSpan CacheDuration => TimeSpan.FromMinutes(10);

    public override string GetCacheKey()
    {
        return CacheKeyGenerator.OrdersList(CustomerId, Page, PageSize);
    }
}