using Microsoft.Extensions.Options;
using Nest;
using ECommerce.ReadModel.Configuration;
using ECommerce.Application.Interfaces;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Base implementation for Elasticsearch operations
/// </summary>
public abstract class BaseElasticsearchService<T> : IElasticsearchService<T> where T : class
{
    protected readonly IElasticClient _elasticClient;
    protected readonly ElasticsearchSettings _settings;
    protected readonly ILogger<BaseElasticsearchService<T>> _logger;
    protected readonly ICacheService? _cacheService;
    protected abstract string IndexName { get; }

    protected BaseElasticsearchService(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> settings,
        ILogger<BaseElasticsearchService<T>> logger,
        ICacheService? cacheService = null)
    {
        _elasticClient = elasticClient;
        _settings = settings.Value;
        _logger = logger;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Gets the full index name with prefix
    /// </summary>
    protected string GetIndexName() => $"{_settings.IndexPrefix}-{IndexName}";

    public virtual async Task<bool> IndexDocumentAsync(T document, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _elasticClient.IndexAsync(document, i => i
                .Index(GetIndexName())
                .Refresh(Elasticsearch.Net.Refresh.WaitFor), cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index document: {Error}", response.OriginalException?.Message ?? response.ServerError?.ToString());
                return false;
            }

            _logger.LogDebug("Successfully indexed document with ID: {Id}", response.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while indexing document");
            return false;
        }
    }

    public virtual async Task<bool> IndexDocumentsAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _elasticClient.BulkAsync(b => b
                .Index(GetIndexName())
                .IndexMany(documents)
                .Refresh(Elasticsearch.Net.Refresh.WaitFor), cancellationToken);

            if (!response.IsValid || response.Errors)
            {
                _logger.LogError("Failed to bulk index documents: {Error}", response.OriginalException?.Message ?? "Bulk operation had errors");
                
                if (response.ItemsWithErrors.Any())
                {
                    foreach (var item in response.ItemsWithErrors)
                    {
                        _logger.LogError("Bulk index error for item {Id}: {Error}", item.Id, item.Error?.Reason);
                    }
                }
                
                return false;
            }

            _logger.LogDebug("Successfully bulk indexed {Count} documents", documents.Count());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while bulk indexing documents");
            return false;
        }
    }

    public virtual async Task<T?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _elasticClient.GetAsync<T>(id.ToString(), g => g
                .Index(GetIndexName()), cancellationToken);

            if (!response.IsValid)
            {
                if (response.Found == false)
                {
                    _logger.LogDebug("Document with ID {Id} not found", id);
                    return null;
                }

                _logger.LogError("Failed to get document with ID {Id}: {Error}", id, response.OriginalException?.Message ?? response.ServerError?.ToString());
                return null;
            }

            return response.Source;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting document with ID {Id}", id);
            return null;
        }
    }

    public virtual async Task<bool> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _elasticClient.DeleteAsync<T>(id.ToString(), d => d
                .Index(GetIndexName())
                .Refresh(Elasticsearch.Net.Refresh.WaitFor), cancellationToken);

            if (!response.IsValid)
            {
                if (response.Result == Result.NotFound)
                {
                    _logger.LogDebug("Document with ID {Id} not found for deletion", id);
                    return true; // Consider not found as successful deletion
                }

                _logger.LogError("Failed to delete document with ID {Id}: {Error}", id, response.OriginalException?.Message ?? response.ServerError?.ToString());
                return false;
            }

            _logger.LogDebug("Successfully deleted document with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while deleting document with ID {Id}", id);
            return false;
        }
    }

    public virtual async Task<ISearchResponse<T>> SearchAsync(SearchDescriptor<T> searchDescriptor, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<T>(searchDescriptor, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Search failed: {Error}", response.OriginalException?.Message ?? response.ServerError?.ToString());
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during search");
            throw;
        }
    }

    /// <summary>
    /// Cached search method for better performance - disabled for now due to Elasticsearch response serialization issues
    /// </summary>
    protected virtual async Task<ISearchResponse<T>> CachedSearchAsync(
        SearchDescriptor<T> searchDescriptor, 
        string cacheKey, 
        TimeSpan cacheDuration,
        CancellationToken cancellationToken = default)
    {
        // Temporarily disable caching for Elasticsearch responses due to serialization issues
        // Elasticsearch SearchResponse contains non-serializable objects like IntPtr
        _logger.LogDebug("Elasticsearch caching disabled - executing direct search for: {CacheKey}", cacheKey);
        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public abstract Task<ISearchResponse<T>> SimpleSearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    public abstract Task<ISearchResponse<T>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default);

    public virtual async Task<bool> IndexExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _elasticClient.Indices.ExistsAsync(GetIndexName(), ct: cancellationToken);
            return response.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking if index exists");
            return false;
        }
    }

    public abstract Task<bool> CreateIndexAsync(CancellationToken cancellationToken = default);

    public virtual async Task<bool> DeleteIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _elasticClient.Indices.DeleteAsync(GetIndexName(), ct: cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete index {IndexName}: {Error}", GetIndexName(), response.OriginalException?.Message ?? response.ServerError?.ToString());
                return false;
            }

            _logger.LogInformation("Successfully deleted index: {IndexName}", GetIndexName());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while deleting index {IndexName}", GetIndexName());
            return false;
        }
    }

    public virtual async Task<bool> RefreshIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _elasticClient.Indices.RefreshAsync(GetIndexName(), ct: cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to refresh index {IndexName}: {Error}", GetIndexName(), response.OriginalException?.Message ?? response.ServerError?.ToString());
                return false;
            }

            _logger.LogDebug("Successfully refreshed index: {IndexName}", GetIndexName());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while refreshing index {IndexName}", GetIndexName());
            return false;
        }
    }
}