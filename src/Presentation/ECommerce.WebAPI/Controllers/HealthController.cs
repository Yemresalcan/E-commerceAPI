using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace ECommerce.WebAPI.Controllers;

/// <summary>
/// Controller for health check operations and monitoring
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class HealthController(
    HealthCheckService healthCheckService,
    ILogger<HealthController> logger) : ControllerBase
{
    /// <summary>
    /// Gets the overall health status of the application
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Health check requested via API");
            
            var healthReport = await healthCheckService.CheckHealthAsync(cancellationToken);
            
            var response = new
            {
                status = healthReport.Status.ToString(),
                totalDuration = healthReport.TotalDuration.TotalMilliseconds,
                timestamp = DateTime.UtcNow,
                checks = healthReport.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.TotalMilliseconds,
                    description = entry.Value.Description,
                    data = entry.Value.Data,
                    tags = entry.Value.Tags
                })
            };

            var statusCode = healthReport.Status == HealthStatus.Healthy 
                ? StatusCodes.Status200OK 
                : StatusCodes.Status503ServiceUnavailable;

            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking health");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                error = "Health check failed",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Gets the health status of a specific component
    /// </summary>
    /// <param name="component">The component name to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Component-specific health status</returns>
    [HttpGet("{component}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetComponentHealth(
        string component, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Component health check requested for: {Component}", component);
            
            var healthReport = await healthCheckService.CheckHealthAsync(
                check => check.Name.Equals(component, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

            if (!healthReport.Entries.Any())
            {
                return NotFound(new
                {
                    error = $"Health check component '{component}' not found",
                    availableComponents = await GetAvailableComponents(cancellationToken)
                });
            }

            var entry = healthReport.Entries.First();
            var response = new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                data = entry.Value.Data,
                tags = entry.Value.Tags,
                timestamp = DateTime.UtcNow
            };

            var statusCode = entry.Value.Status == HealthStatus.Healthy 
                ? StatusCodes.Status200OK 
                : StatusCodes.Status503ServiceUnavailable;

            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking component health for: {Component}", component);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                component,
                status = "Unhealthy",
                error = "Component health check failed",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Gets a list of available health check components
    /// </summary>
    /// <returns>List of available components</returns>
    [HttpGet("components")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableComponents(CancellationToken cancellationToken = default)
    {
        try
        {
            var healthReport = await healthCheckService.CheckHealthAsync(cancellationToken);
            
            var components = healthReport.Entries.Select(entry => new
            {
                name = entry.Key,
                tags = entry.Value.Tags,
                description = entry.Value.Description
            }).ToList();

            return Ok(new
            {
                totalComponents = components.Count,
                components,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting available components");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Failed to retrieve available components",
                timestamp = DateTime.UtcNow
            });
        }
    }
}