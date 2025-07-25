using ECommerce.ReadModel.Models;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Implementation of index management service
/// </summary>
public class IndexManagementService : IIndexManagementService
{
    private readonly IProductSearchService _productSearchService;
    private readonly IOrderSearchService _orderSearchService;
    private readonly ICustomerSearchService _customerSearchService;
    private readonly ILogger<IndexManagementService> _logger;

    public IndexManagementService(
        IProductSearchService productSearchService,
        IOrderSearchService orderSearchService,
        ICustomerSearchService customerSearchService,
        ILogger<IndexManagementService> logger)
    {
        _productSearchService = productSearchService;
        _orderSearchService = orderSearchService;
        _customerSearchService = customerSearchService;
        _logger = logger;
    }

    public async Task<bool> EnsureAllIndicesCreatedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ensuring all Elasticsearch indices are created...");

        var tasks = new[]
        {
            EnsureIndexCreatedAsync(_productSearchService, "Products", cancellationToken),
            EnsureIndexCreatedAsync(_orderSearchService, "Orders", cancellationToken),
            EnsureIndexCreatedAsync(_customerSearchService, "Customers", cancellationToken)
        };

        var results = await Task.WhenAll(tasks);
        var success = results.All(r => r);

        if (success)
        {
            _logger.LogInformation("All Elasticsearch indices are ready");
        }
        else
        {
            _logger.LogError("Failed to ensure some Elasticsearch indices are created");
        }

        return success;
    }

    public async Task<bool> CreateAllIndicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating all Elasticsearch indices...");

        var tasks = new[]
        {
            CreateIndexAsync(_productSearchService, "Products", cancellationToken),
            CreateIndexAsync(_orderSearchService, "Orders", cancellationToken),
            CreateIndexAsync(_customerSearchService, "Customers", cancellationToken)
        };

        var results = await Task.WhenAll(tasks);
        var success = results.All(r => r);

        if (success)
        {
            _logger.LogInformation("All Elasticsearch indices created successfully");
        }
        else
        {
            _logger.LogError("Failed to create some Elasticsearch indices");
        }

        return success;
    }

    public async Task<bool> DeleteAllIndicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Deleting all Elasticsearch indices...");

        var tasks = new[]
        {
            DeleteIndexAsync(_productSearchService, "Products", cancellationToken),
            DeleteIndexAsync(_orderSearchService, "Orders", cancellationToken),
            DeleteIndexAsync(_customerSearchService, "Customers", cancellationToken)
        };

        var results = await Task.WhenAll(tasks);
        var success = results.All(r => r);

        if (success)
        {
            _logger.LogInformation("All Elasticsearch indices deleted successfully");
        }
        else
        {
            _logger.LogError("Failed to delete some Elasticsearch indices");
        }

        return success;
    }

    public async Task<bool> RecreateAllIndicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recreating all Elasticsearch indices...");

        var deleteSuccess = await DeleteAllIndicesAsync(cancellationToken);
        if (!deleteSuccess)
        {
            _logger.LogError("Failed to delete indices during recreation");
            return false;
        }

        // Wait a bit for deletion to complete
        await Task.Delay(1000, cancellationToken);

        var createSuccess = await CreateAllIndicesAsync(cancellationToken);
        if (!createSuccess)
        {
            _logger.LogError("Failed to create indices during recreation");
            return false;
        }

        _logger.LogInformation("All Elasticsearch indices recreated successfully");
        return true;
    }

    public async Task<IndexHealthStatus> CheckIndicesHealthAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking health of all Elasticsearch indices...");

        var healthStatus = new IndexHealthStatus();

        var tasks = new[]
        {
            CheckIndexHealthAsync(_productSearchService, "Products", cancellationToken),
            CheckIndexHealthAsync(_orderSearchService, "Orders", cancellationToken),
            CheckIndexHealthAsync(_customerSearchService, "Customers", cancellationToken)
        };

        var results = await Task.WhenAll(tasks);

        foreach (var (indexName, isHealthy, error) in results)
        {
            healthStatus.IndexStatus[indexName] = isHealthy;
            if (!isHealthy && !string.IsNullOrEmpty(error))
            {
                healthStatus.Errors.Add($"{indexName}: {error}");
            }
        }

        healthStatus.IsHealthy = healthStatus.IndexStatus.Values.All(status => status);

        if (healthStatus.IsHealthy)
        {
            _logger.LogDebug("All Elasticsearch indices are healthy");
        }
        else
        {
            _logger.LogWarning("Some Elasticsearch indices are not healthy: {Errors}", string.Join(", ", healthStatus.Errors));
        }

        return healthStatus;
    }

    public async Task<bool> RefreshAllIndicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Refreshing all Elasticsearch indices...");

        var tasks = new[]
        {
            RefreshIndexAsync(_productSearchService, "Products", cancellationToken),
            RefreshIndexAsync(_orderSearchService, "Orders", cancellationToken),
            RefreshIndexAsync(_customerSearchService, "Customers", cancellationToken)
        };

        var results = await Task.WhenAll(tasks);
        var success = results.All(r => r);

        if (success)
        {
            _logger.LogDebug("All Elasticsearch indices refreshed successfully");
        }
        else
        {
            _logger.LogWarning("Failed to refresh some Elasticsearch indices");
        }

        return success;
    }

    private async Task<bool> EnsureIndexCreatedAsync<T>(IElasticsearchService<T> service, string indexName, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var exists = await service.IndexExistsAsync(cancellationToken);
            if (exists)
            {
                _logger.LogDebug("{IndexName} index already exists", indexName);
                return true;
            }

            _logger.LogInformation("Creating {IndexName} index...", indexName);
            var created = await service.CreateIndexAsync(cancellationToken);
            
            if (created)
            {
                _logger.LogInformation("{IndexName} index created successfully", indexName);
            }
            else
            {
                _logger.LogError("Failed to create {IndexName} index", indexName);
            }

            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while ensuring {IndexName} index is created", indexName);
            return false;
        }
    }

    private async Task<bool> CreateIndexAsync<T>(IElasticsearchService<T> service, string indexName, CancellationToken cancellationToken) where T : class
    {
        try
        {
            _logger.LogInformation("Creating {IndexName} index...", indexName);
            var created = await service.CreateIndexAsync(cancellationToken);
            
            if (created)
            {
                _logger.LogInformation("{IndexName} index created successfully", indexName);
            }
            else
            {
                _logger.LogError("Failed to create {IndexName} index", indexName);
            }

            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating {IndexName} index", indexName);
            return false;
        }
    }

    private async Task<bool> DeleteIndexAsync<T>(IElasticsearchService<T> service, string indexName, CancellationToken cancellationToken) where T : class
    {
        try
        {
            _logger.LogInformation("Deleting {IndexName} index...", indexName);
            var deleted = await service.DeleteIndexAsync(cancellationToken);
            
            if (deleted)
            {
                _logger.LogInformation("{IndexName} index deleted successfully", indexName);
            }
            else
            {
                _logger.LogError("Failed to delete {IndexName} index", indexName);
            }

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while deleting {IndexName} index", indexName);
            return false;
        }
    }

    private async Task<(string IndexName, bool IsHealthy, string? Error)> CheckIndexHealthAsync<T>(IElasticsearchService<T> service, string indexName, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var exists = await service.IndexExistsAsync(cancellationToken);
            return (indexName, exists, exists ? null : "Index does not exist");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking health of {IndexName} index", indexName);
            return (indexName, false, ex.Message);
        }
    }

    private async Task<bool> RefreshIndexAsync<T>(IElasticsearchService<T> service, string indexName, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var refreshed = await service.RefreshIndexAsync(cancellationToken);
            
            if (refreshed)
            {
                _logger.LogDebug("{IndexName} index refreshed successfully", indexName);
            }
            else
            {
                _logger.LogWarning("Failed to refresh {IndexName} index", indexName);
            }

            return refreshed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while refreshing {IndexName} index", indexName);
            return false;
        }
    }
}