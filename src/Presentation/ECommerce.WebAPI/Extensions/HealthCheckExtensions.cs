using ECommerce.WebAPI.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace ECommerce.WebAPI.Extensions;

/// <summary>
/// Extension methods for configuring health checks
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds comprehensive health checks for all application dependencies
    /// </summary>
    public static IServiceCollection AddComprehensiveHealthChecks(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Add PostgreSQL health check
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecksBuilder.AddNpgSql(
                connectionString,
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["database", "postgresql"]);
        }

        // Add Redis health check
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            healthChecksBuilder.AddRedis(
                redisConnectionString,
                name: "redis",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["cache", "redis"]);
        }

        // Add RabbitMQ health check
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ");
        if (!string.IsNullOrEmpty(rabbitMqConnectionString))
        {
            try
            {
                // Validate URI format before adding health check
                var uri = new Uri(rabbitMqConnectionString);
                healthChecksBuilder.AddRabbitMQ(
                    rabbitMqConnectionString,
                    name: "rabbitmq",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["messaging", "rabbitmq"]);
            }
            catch (UriFormatException)
            {
                // Skip RabbitMQ health check if connection string is invalid
                // Note: Logger is not available in this context, so we'll skip logging
            }
        }

        // Add Elasticsearch health check
        healthChecksBuilder.AddCheck<ElasticsearchHealthCheck>(
            "elasticsearch",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["search", "elasticsearch"]);

        // Add application services health check
        healthChecksBuilder.AddCheck<ApplicationHealthCheck>(
            "application",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["application", "services"]);

        // Add system resources health check
        healthChecksBuilder.AddCheck<SystemResourcesHealthCheck>(
            "system-resources",
            failureStatus: HealthStatus.Degraded,
            tags: ["system", "resources"]);

        // Register custom health check implementations
        services.AddScoped<ElasticsearchHealthCheck>();
        services.AddScoped<ApplicationHealthCheck>();
        services.AddScoped<SystemResourcesHealthCheck>();

        // Add Health Checks UI
        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30); // Evaluate health checks every 30 seconds
            options.MaximumHistoryEntriesPerEndpoint(50); // Keep 50 history entries per endpoint
            options.AddHealthCheckEndpoint("ECommerce API", "/health/detailed");
        })
        .AddInMemoryStorage(); // Use in-memory storage for simplicity

        return services;
    }

    /// <summary>
    /// Maps health check endpoints with different levels of detail
    /// </summary>
    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Basic health check endpoint
        app.MapHealthChecks("/health");

        // Detailed health check endpoint with JSON response
        app.MapHealthChecks("/health/detailed", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Ready endpoint (for Kubernetes readiness probes)
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        // Live endpoint (for Kubernetes liveness probes)
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false // Only basic application liveness
        });

        // Map Health Checks UI
        app.MapHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
        });

        return app;
    }
}