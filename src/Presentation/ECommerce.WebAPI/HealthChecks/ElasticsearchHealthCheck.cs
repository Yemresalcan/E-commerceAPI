using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Nest;
using ECommerce.ReadModel.Configuration;

namespace ECommerce.WebAPI.HealthChecks;

/// <summary>
/// Health check for Elasticsearch connectivity and cluster health
/// </summary>
public class ElasticsearchHealthCheck(
    IElasticClient elasticClient,
    IOptions<ElasticsearchSettings> options,
    ILogger<ElasticsearchHealthCheck> logger) : IHealthCheck
{
    private readonly ElasticsearchSettings _config = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking Elasticsearch health");

            // Check if Elasticsearch is reachable
            var pingResponse = await elasticClient.PingAsync(ct: cancellationToken);
            if (!pingResponse.IsValid)
            {
                logger.LogWarning("Elasticsearch ping failed: {Error}", pingResponse.OriginalException?.Message);
                return HealthCheckResult.Unhealthy(
                    "Elasticsearch is not reachable",
                    pingResponse.OriginalException);
            }

            // Check cluster health
            var clusterHealthResponse = await elasticClient.Cluster.HealthAsync(ct: cancellationToken);
            if (!clusterHealthResponse.IsValid)
            {
                logger.LogWarning("Elasticsearch cluster health check failed: {Error}", 
                    clusterHealthResponse.OriginalException?.Message);
                return HealthCheckResult.Degraded(
                    "Elasticsearch cluster health check failed",
                    clusterHealthResponse.OriginalException);
            }

            var clusterHealth = clusterHealthResponse.Status;
            var data = new Dictionary<string, object>
            {
                ["cluster_name"] = clusterHealthResponse.ClusterName,
                ["status"] = clusterHealth.ToString(),
                ["number_of_nodes"] = clusterHealthResponse.NumberOfNodes,
                ["number_of_data_nodes"] = clusterHealthResponse.NumberOfDataNodes,
                ["active_primary_shards"] = clusterHealthResponse.ActivePrimaryShards,
                ["active_shards"] = clusterHealthResponse.ActiveShards,
                ["relocating_shards"] = clusterHealthResponse.RelocatingShards,
                ["initializing_shards"] = clusterHealthResponse.InitializingShards,
                ["unassigned_shards"] = clusterHealthResponse.UnassignedShards,
                ["uri"] = _config.Uri
            };

            var healthStatus = clusterHealth.ToString().ToLowerInvariant();
            return healthStatus switch
            {
                "green" => HealthCheckResult.Healthy("Elasticsearch cluster is healthy", data),
                "yellow" => HealthCheckResult.Degraded("Elasticsearch cluster is in yellow state", null, data),
                "red" => HealthCheckResult.Unhealthy("Elasticsearch cluster is in red state", null, data),
                _ => HealthCheckResult.Unhealthy("Unknown Elasticsearch cluster state", null, data)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Elasticsearch health");
            return HealthCheckResult.Unhealthy("Error checking Elasticsearch health", ex);
        }
    }
}