using ECommerce.Application.Interfaces;
using ECommerce.Domain.Aggregates;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Infrastructure.Caching.Decorators;

/// <summary>
/// Helper class for caching boolean results
/// </summary>
public class ExistsResult
{
    public bool Exists { get; set; }
}

/// <summary>
/// Caching decorator for repositories to improve read performance
/// </summary>
/// <typeparam name="T">The aggregate root type</typeparam>
public class CachedRepositoryDecorator<T> : IRepository<T> where T : AggregateRoot
{
    private readonly IRepository<T> _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedRepositoryDecorator<T>> _logger;
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(15);
    private readonly string _cacheKeyPrefix;

    public CachedRepositoryDecorator(
        IRepository<T> repository,
        ICacheService cacheService,
        ILogger<CachedRepositoryDecorator<T>> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheKeyPrefix = $"{typeof(T).Name.ToLowerInvariant()}";
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{_cacheKeyPrefix}:id:{id}";
        
        var cached = await _cacheService.GetAsync<T>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for {AggregateType} with ID: {Id}", typeof(T).Name, id);
            return cached;
        }

        _logger.LogDebug("Cache miss for {AggregateType} with ID: {Id}", typeof(T).Name, id);
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (entity != null)
        {
            await _cacheService.SetAsync(cacheKey, entity, _defaultCacheDuration, cancellationToken);
            _logger.LogDebug("Cached {AggregateType} with ID: {Id}", typeof(T).Name, id);
        }

        return entity;
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{_cacheKeyPrefix}:all";
        
        var cached = await _cacheService.GetAsync<List<T>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for all {AggregateType} entities", typeof(T).Name);
            return cached;
        }

        _logger.LogDebug("Cache miss for all {AggregateType} entities", typeof(T).Name);
        var entities = await _repository.GetAllAsync(cancellationToken);
        var entitiesList = entities.ToList();
        
        if (entitiesList.Any())
        {
            await _cacheService.SetAsync(cacheKey, entitiesList, TimeSpan.FromMinutes(5), cancellationToken);
            _logger.LogDebug("Cached {Count} {AggregateType} entities", entitiesList.Count, typeof(T).Name);
        }

        return entitiesList;
    }

    public async Task AddAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(aggregate, cancellationToken);
        await InvalidateCacheAsync(aggregate.Id);
    }

    public void Update(T aggregate)
    {
        _repository.Update(aggregate);
        // Note: Cache invalidation will happen in UnitOfWork after successful save
    }

    public void Delete(T aggregate)
    {
        _repository.Delete(aggregate);
        // Note: Cache invalidation will happen in UnitOfWork after successful save
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteByIdAsync(id, cancellationToken);
        await InvalidateCacheAsync(id);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{_cacheKeyPrefix}:exists:{id}";
        
        var cached = await _cacheService.GetAsync<ExistsResult>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for {AggregateType} exists check with ID: {Id}", typeof(T).Name, id);
            return cached.Exists;
        }

        _logger.LogDebug("Cache miss for {AggregateType} exists check with ID: {Id}", typeof(T).Name, id);
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        
        await _cacheService.SetAsync(cacheKey, new ExistsResult { Exists = exists }, TimeSpan.FromMinutes(30), cancellationToken);
        _logger.LogDebug("Cached exists result for {AggregateType} with ID: {Id} = {Exists}", typeof(T).Name, id, exists);

        return exists;
    }

    public async Task<IEnumerable<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        // For complex specifications, we don't cache as cache keys would be too complex
        return await _repository.FindAsync(specification, cancellationToken);
    }

    public async Task<T?> FindSingleAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        // For complex specifications, we don't cache as cache keys would be too complex
        return await _repository.FindSingleAsync(specification, cancellationToken);
    }

    public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        // For complex specifications, we don't cache as cache keys would be too complex
        return await _repository.CountAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Invalidates cache entries for a specific aggregate
    /// </summary>
    public async Task InvalidateCacheAsync(Guid id)
    {
        var patterns = new[]
        {
            $"{_cacheKeyPrefix}:id:{id}",
            $"{_cacheKeyPrefix}:exists:{id}",
            $"{_cacheKeyPrefix}:all",
            $"{_cacheKeyPrefix}:*" // Invalidate all related cache entries
        };

        foreach (var pattern in patterns)
        {
            if (pattern.Contains('*'))
            {
                await _cacheService.RemoveByPatternAsync(pattern);
            }
            else
            {
                await _cacheService.RemoveAsync(pattern);
            }
        }

        _logger.LogDebug("Invalidated cache for {AggregateType} with ID: {Id}", typeof(T).Name, id);
    }

    /// <summary>
    /// Invalidates all cache entries for this aggregate type
    /// </summary>
    public async Task InvalidateAllCacheAsync()
    {
        await _cacheService.RemoveByPatternAsync($"{_cacheKeyPrefix}:*");
        _logger.LogDebug("Invalidated all cache for {AggregateType}", typeof(T).Name);
    }
}