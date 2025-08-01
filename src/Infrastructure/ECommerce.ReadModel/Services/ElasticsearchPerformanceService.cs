using Nest;
using ECommerce.ReadModel.Configuration;
using Microsoft.Extensions.Options;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Service for monitoring and optimizing Elasticsearch performance
/// </summary>
public class ElasticsearchPerformanceService
{
    private readonly IElasticClient _elasticClient;
    private readonly ElasticsearchSettings _settings;
    private readonly ILogger<ElasticsearchPerformanceService> _logger;

    public ElasticsearchPerformanceService(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> settings,
        ILogger<ElasticsearchPerformanceService> logger)
    {
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Optimizes index settings for better performance
    /// </summary>
    public async Task OptimizeIndexSettingsAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var updateSettingsResponse = await _elasticClient.Indices.UpdateSettingsAsync(indexName, u => u
                .IndexSettings(s => s
                    .RefreshInterval("30s") // Optimize refresh interval
                    .NumberOfReplicas(1) // Ensure proper replication
                    .Setting("index.merge.policy.max_merge_at_once", 10)
                    .Setting("index.merge.policy.segments_per_tier", 10)
                    .Setting("index.merge.scheduler.max_thread_count", 1)
                    .Setting("index.search.idle.after", "30s")
                    .Setting("index.blocks.read_only_allow_delete", false)
                ), cancellationToken);

            if (updateSettingsResponse.IsValid)
            {
                _logger.LogInformation("Successfully optimized settings for index {IndexName}", indexName);
            }
            else
            {
                _logger.LogWarning("Failed to optimize settings for index {IndexName}: {Error}", 
                    indexName, updateSettingsResponse.OriginalException?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing index settings for {IndexName}", indexName);
        }
    }

    /// <summary>
    /// Forces merge of index segments for better search performance
    /// </summary>
    public async Task OptimizeIndexSegmentsAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var forceMergeResponse = await _elasticClient.Indices.ForceMergeAsync(indexName, f => f
                .MaxNumSegments(1) // Merge to single segment for optimal read performance
                .OnlyExpungeDeletes(false)
                .Flush(true)
            , cancellationToken);

            if (forceMergeResponse.IsValid)
            {
                _logger.LogInformation("Successfully optimized segments for index {IndexName}", indexName);
            }
            else
            {
                _logger.LogWarning("Failed to optimize segments for index {IndexName}: {Error}", 
                    indexName, forceMergeResponse.OriginalException?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing index segments for {IndexName}", indexName);
        }
    }

    /// <summary>
    /// Clears field data cache to free memory
    /// </summary>
    public async Task ClearFieldDataCacheAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var clearCacheResponse = await _elasticClient.Indices.ClearCacheAsync(indexName, c => c
                .Query(true)
                .Request(true)
            , cancellationToken);

            if (clearCacheResponse.IsValid)
            {
                _logger.LogInformation("Successfully cleared cache for index {IndexName}", indexName);
            }
            else
            {
                _logger.LogWarning("Failed to clear cache for index {IndexName}: {Error}", 
                    indexName, clearCacheResponse.OriginalException?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache for index {IndexName}", indexName);
        }
    }

    /// <summary>
    /// Checks if index exists and is accessible
    /// </summary>
    public async Task<bool> CheckIndexExistsAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var existsResponse = await _elasticClient.Indices.ExistsAsync(indexName);
            
            if (existsResponse.Exists)
            {
                _logger.LogDebug("Index {IndexName} exists and is accessible", indexName);
                return true;
            }
            
            _logger.LogWarning("Index {IndexName} does not exist", indexName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if index exists: {IndexName}", indexName);
            return false;
        }
    }

    /// <summary>
    /// Monitors index performance by checking basic operations
    /// </summary>
    public async Task MonitorIndexPerformanceAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await CheckIndexExistsAsync(indexName, cancellationToken);
            
            if (!exists)
            {
                _logger.LogWarning("Index {IndexName} is not accessible - consider recreation", indexName);
            }
            else
            {
                _logger.LogDebug("Index {IndexName} is accessible", indexName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring performance for index {IndexName}", indexName);
        }
    }

    /// <summary>
    /// Warms up the index by executing common queries
    /// </summary>
    public async Task WarmupIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Execute common search patterns to warm up caches
            var warmupQueries = new[]
            {
                "*", // Match all
                "featured:true", // Featured products
                "category:electronics", // Popular category
                "price:[0 TO 100]" // Price range
            };

            foreach (var query in warmupQueries)
            {
                await _elasticClient.SearchAsync<object>(s => s
                    .Index(indexName)
                    .Size(1) // Minimal result set
                    .Query(q => q.QueryString(qs => qs.Query(query)))
                    .RequestCache(true)
                , cancellationToken);
            }

            _logger.LogInformation("Successfully warmed up index {IndexName}", indexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up index {IndexName}", indexName);
        }
    }


}