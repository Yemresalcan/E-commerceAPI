using Microsoft.Extensions.Diagnostics.HealthChecks;
using ECommerce.Application.Interfaces;

namespace ECommerce.WebAPI.HealthChecks;

/// <summary>
/// Health check for application services and dependencies
/// </summary>
public class ApplicationHealthCheck(
    IProductQueryService productQueryService,
    IOrderQueryService orderQueryService,
    ICustomerQueryService customerQueryService,
    ILogger<ApplicationHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking application services health");

            var healthData = new Dictionary<string, object>();
            var issues = new List<string>();

            // Check product query service
            try
            {
                await productQueryService.CheckHealthAsync(cancellationToken);
                healthData["product_query_service"] = "healthy";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Product query service health check failed");
                healthData["product_query_service"] = "unhealthy";
                issues.Add($"Product query service: {ex.Message}");
            }

            // Check order query service
            try
            {
                await orderQueryService.CheckHealthAsync(cancellationToken);
                healthData["order_query_service"] = "healthy";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Order query service health check failed");
                healthData["order_query_service"] = "unhealthy";
                issues.Add($"Order query service: {ex.Message}");
            }

            // Check customer query service
            try
            {
                await customerQueryService.CheckHealthAsync(cancellationToken);
                healthData["customer_query_service"] = "healthy";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Customer query service health check failed");
                healthData["customer_query_service"] = "unhealthy";
                issues.Add($"Customer query service: {ex.Message}");
            }

            healthData["timestamp"] = DateTime.UtcNow;
            healthData["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";

            if (issues.Count == 0)
            {
                return HealthCheckResult.Healthy("All application services are healthy", healthData);
            }

            if (issues.Count < 3)
            {
                return HealthCheckResult.Degraded(
                    $"Some application services are unhealthy: {string.Join(", ", issues)}", 
                    null, 
                    healthData);
            }

            return HealthCheckResult.Unhealthy(
                $"Multiple application services are unhealthy: {string.Join(", ", issues)}", 
                null, 
                healthData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking application services health");
            return HealthCheckResult.Unhealthy("Error checking application services health", ex);
        }
    }
}