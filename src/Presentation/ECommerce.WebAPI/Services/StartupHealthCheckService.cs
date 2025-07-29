using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerce.WebAPI.Services;

/// <summary>
/// Service that performs health checks during application startup
/// </summary>
public class StartupHealthCheckService(
    HealthCheckService healthCheckService,
    ILogger<StartupHealthCheckService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Performing startup health checks...");
        
        try
        {
            var healthReport = await healthCheckService.CheckHealthAsync(cancellationToken);
            
            if (healthReport.Status == HealthStatus.Healthy)
            {
                logger.LogInformation("All health checks passed successfully");
            }
            else
            {
                logger.LogWarning("Some health checks failed during startup:");
                foreach (var entry in healthReport.Entries.Where(e => e.Value.Status != HealthStatus.Healthy))
                {
                    logger.LogWarning("- {HealthCheckName}: {Status} - {Description}", 
                        entry.Key, entry.Value.Status, entry.Value.Description);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during startup health checks");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}