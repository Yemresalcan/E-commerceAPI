using Microsoft.Extensions.Options;
using ECommerce.WebAPI.Extensions;

namespace ECommerce.WebAPI.Services;

/// <summary>
/// Service for validating configuration on application startup
/// </summary>
public class ConfigurationValidationService(
    IOptions<DatabaseOptions> databaseOptions,
    IOptions<ElasticsearchOptions> elasticsearchOptions,
    IOptions<RabbitMQOptions> rabbitMqOptions,
    IOptions<CacheOptions> cacheOptions,
    ILogger<ConfigurationValidationService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            ValidateConfiguration();
            logger.LogInformation("Configuration validation completed successfully");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Configuration validation failed");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void ValidateConfiguration()
    {
        // Validate database configuration
        var dbConfig = databaseOptions.Value;
        if (string.IsNullOrWhiteSpace(dbConfig.DefaultConnection))
        {
            throw new InvalidOperationException("Database connection string is required");
        }

        // Validate Elasticsearch configuration
        var esConfig = elasticsearchOptions.Value;
        if (string.IsNullOrWhiteSpace(esConfig.Uri))
        {
            throw new InvalidOperationException("Elasticsearch URI is required");
        }

        if (string.IsNullOrWhiteSpace(esConfig.IndexPrefix))
        {
            throw new InvalidOperationException("Elasticsearch index prefix is required");
        }

        // Validate RabbitMQ configuration
        var rmqConfig = rabbitMqOptions.Value;
        if (string.IsNullOrWhiteSpace(rmqConfig.HostName))
        {
            throw new InvalidOperationException("RabbitMQ hostname is required");
        }

        if (string.IsNullOrWhiteSpace(rmqConfig.UserName))
        {
            throw new InvalidOperationException("RabbitMQ username is required");
        }

        if (string.IsNullOrWhiteSpace(rmqConfig.ExchangeName))
        {
            throw new InvalidOperationException("RabbitMQ exchange name is required");
        }

        // Validate cache configuration
        var cacheConfig = cacheOptions.Value;
        if (string.IsNullOrWhiteSpace(cacheConfig.KeyPrefix))
        {
            throw new InvalidOperationException("Cache key prefix is required");
        }

        // Log configuration summary
        logger.LogInformation("Configuration validation summary:");
        logger.LogInformation("- Database: PostgreSQL configured");
        logger.LogInformation("- Cache: Redis configured with prefix '{CachePrefix}'", cacheConfig.KeyPrefix);
        logger.LogInformation("- Search: Elasticsearch configured at '{ElasticsearchUri}'", esConfig.Uri);
        logger.LogInformation("- Messaging: RabbitMQ configured at '{RabbitMQHost}:{RabbitMQPort}'", 
            rmqConfig.HostName, rmqConfig.Port);
    }
}