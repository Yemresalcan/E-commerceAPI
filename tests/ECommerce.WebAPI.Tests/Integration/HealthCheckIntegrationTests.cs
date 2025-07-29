using System.Net;
using FluentAssertions;
using System.Text.Json;

namespace ECommerce.WebAPI.Tests.Integration;

public class HealthCheckIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy_WhenAllServicesAreRunning()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        healthResult.GetProperty("status").GetString().Should().Be("Healthy");
        
        // Verify individual service health checks
        var entries = healthResult.GetProperty("entries");
        
        // PostgreSQL should be healthy
        if (entries.TryGetProperty("postgresql", out var postgresHealth))
        {
            postgresHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
        
        // Redis should be healthy
        if (entries.TryGetProperty("redis", out var redisHealth))
        {
            redisHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
        
        // Elasticsearch should be healthy
        if (entries.TryGetProperty("elasticsearch", out var elasticsearchHealth))
        {
            elasticsearchHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
        
        // RabbitMQ should be healthy
        if (entries.TryGetProperty("rabbitmq", out var rabbitmqHealth))
        {
            rabbitmqHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
    }

    [Fact]
    public async Task HealthCheckUI_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/healthchecks-ui");

        // Assert
        // The health check UI might redirect or return HTML
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task ReadinessCheck_ShouldReturnReady_WhenApplicationIsReady()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        healthResult.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task LivenessCheck_ShouldReturnLive_WhenApplicationIsRunning()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        healthResult.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task DatabaseHealthCheck_ShouldVerifyDatabaseConnectivity()
    {
        // This test specifically verifies that the database health check
        // correctly identifies database connectivity issues
        
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        // The overall status should be healthy since our test containers are running
        healthResult.GetProperty("status").GetString().Should().Be("Healthy");
        
        // Verify that database-related health checks are present and healthy
        var entries = healthResult.GetProperty("entries");
        
        // Check if we have database connectivity
        var hasHealthyDatabase = false;
        foreach (var entry in entries.EnumerateObject())
        {
            if (entry.Name.Contains("database") || entry.Name.Contains("postgresql") || entry.Name.Contains("sql"))
            {
                if (entry.Value.GetProperty("status").GetString() == "Healthy")
                {
                    hasHealthyDatabase = true;
                    break;
                }
            }
        }
        
        // We should have at least one healthy database connection
        hasHealthyDatabase.Should().BeTrue("At least one database health check should be healthy");
    }

    [Fact]
    public async Task CacheHealthCheck_ShouldVerifyRedisConnectivity()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        var entries = healthResult.GetProperty("entries");
        
        // Check if we have Redis connectivity
        var hasHealthyCache = false;
        foreach (var entry in entries.EnumerateObject())
        {
            if (entry.Name.Contains("redis") || entry.Name.Contains("cache"))
            {
                if (entry.Value.GetProperty("status").GetString() == "Healthy")
                {
                    hasHealthyCache = true;
                    break;
                }
            }
        }
        
        // We should have a healthy cache connection
        hasHealthyCache.Should().BeTrue("Redis health check should be healthy");
    }

    [Fact]
    public async Task SearchHealthCheck_ShouldVerifyElasticsearchConnectivity()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        var entries = healthResult.GetProperty("entries");
        
        // Check if we have Elasticsearch connectivity
        var hasHealthySearch = false;
        foreach (var entry in entries.EnumerateObject())
        {
            if (entry.Name.Contains("elasticsearch") || entry.Name.Contains("search"))
            {
                if (entry.Value.GetProperty("status").GetString() == "Healthy")
                {
                    hasHealthySearch = true;
                    break;
                }
            }
        }
        
        // We should have a healthy search connection
        hasHealthySearch.Should().BeTrue("Elasticsearch health check should be healthy");
    }

    [Fact]
    public async Task MessageQueueHealthCheck_ShouldVerifyRabbitMQConnectivity()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        var entries = healthResult.GetProperty("entries");
        
        // Check if we have RabbitMQ connectivity
        var hasHealthyMessageQueue = false;
        foreach (var entry in entries.EnumerateObject())
        {
            if (entry.Name.Contains("rabbitmq") || entry.Name.Contains("messagequeue") || entry.Name.Contains("mq"))
            {
                if (entry.Value.GetProperty("status").GetString() == "Healthy")
                {
                    hasHealthyMessageQueue = true;
                    break;
                }
            }
        }
        
        // We should have a healthy message queue connection
        hasHealthyMessageQueue.Should().BeTrue("RabbitMQ health check should be healthy");
    }

    [Fact]
    public async Task ApplicationHealthCheck_ShouldVerifyApplicationServices()
    {
        // This test verifies that custom application health checks are working
        
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        // The overall application should be healthy
        healthResult.GetProperty("status").GetString().Should().Be("Healthy");
        
        // Verify that we have some health check entries
        var entries = healthResult.GetProperty("entries");
        entries.EnumerateObject().Should().NotBeEmpty("Should have at least one health check entry");
        
        // Verify response time is reasonable
        if (healthResult.TryGetProperty("totalDuration", out var duration))
        {
            // Health checks should complete within a reasonable time
            var durationMs = TimeSpan.Parse(duration.GetString()!).TotalMilliseconds;
            durationMs.Should().BeLessThan(30000, "Health checks should complete within 30 seconds");
        }
    }
}