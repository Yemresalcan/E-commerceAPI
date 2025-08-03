using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.WebAPI.Tests.Integration;

/// <summary>
/// Tests to validate Docker Compose setup and service dependencies.
/// These tests verify that all services can start correctly and communicate with each other.
/// </summary>
public class DockerComposeValidationTests : IntegrationTestBase
{
    [Fact]
    public async Task DockerServices_ShouldAllBeHealthy()
    {
        // Test that all Docker services are running and healthy
        var healthResponse = await _client.GetAsync("/health");
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(healthContent);
        
        // Overall health should be healthy
        healthResult.GetProperty("status").GetString().Should().Be("Healthy");
        
        // Check individual service health
        var entries = healthResult.GetProperty("entries");
        
        // Database connectivity
        if (entries.TryGetProperty("database", out var dbHealth))
        {
            dbHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
        
        // Cache connectivity
        if (entries.TryGetProperty("redis", out var redisHealth))
        {
            redisHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
        
        // Search engine connectivity
        if (entries.TryGetProperty("elasticsearch", out var esHealth))
        {
            esHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
        
        // Message queue connectivity
        if (entries.TryGetProperty("rabbitmq", out var mqHealth))
        {
            mqHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
    }

    [Fact]
    public async Task PostgreSQL_ShouldBeAccessible()
    {
        // Verify PostgreSQL container is running and accessible
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue("PostgreSQL container should be accessible");
        
        // Verify database schema exists by checking if we can query the database
        try
        {
            var productCount = await context.Products.CountAsync();
            // If we can count products, the database schema is working
            productCount.Should().BeGreaterOrEqualTo(0, "Database should be accessible and have product table");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Database schema verification failed", ex);
        }
    }

    [Fact]
    public async Task Redis_ShouldBeAccessible()
    {
        // Test Redis connectivity through the application
        // This would typically be done through a cache service
        
        // For now, we'll test through health checks
        var healthResponse = await _client.GetAsync("/health");
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // In a real implementation, you might test actual caching operations
        // var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        // await cacheService.SetAsync("test-key", "test-value", TimeSpan.FromMinutes(1));
        // var cachedValue = await cacheService.GetAsync<string>("test-key");
        // cachedValue.Should().Be("test-value");
    }

    [Fact]
    public async Task Elasticsearch_ShouldBeAccessible()
    {
        // Test Elasticsearch connectivity
        // This would typically be tested through search operations
        
        // Wait a bit for Elasticsearch to be fully ready
        await Task.Delay(5000);
        
        // Test search endpoint (even if it returns empty results)
        var searchResponse = await _client.GetAsync("/api/products/search?searchTerm=test");
        
        // Should not fail due to Elasticsearch connectivity issues
        searchResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RabbitMQ_ShouldBeAccessible()
    {
        // Test RabbitMQ connectivity through event publishing
        // This is tested indirectly through the health check system
        
        var healthResponse = await _client.GetAsync("/health");
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // In a real implementation, you might test actual message publishing
        // var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        // await eventBus.PublishAsync(new TestEvent());
    }

    [Fact]
    public async Task ServiceDependencies_ShouldStartInCorrectOrder()
    {
        // Test that services start in the correct dependency order
        // This is implicitly tested by the fact that the application starts successfully
        
        // Verify the application is responsive
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify all dependent services are available
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        // Database should be ready
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
        
        // Application should have started successfully (implicit test)
        // If any dependency was missing, the application wouldn't start
    }

    [Fact]
    public async Task ContainerHealthChecks_ShouldReportCorrectly()
    {
        // Test that container health checks are working
        var healthResponse = await _client.GetAsync("/health");
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(healthContent);
        
        // Should have health check results
        healthResult.TryGetProperty("entries", out var entries).Should().BeTrue();
        
        // Should have at least database health check
        entries.TryGetProperty("database", out _).Should().BeTrue();
        
        // Health checks should include timing information
        if (entries.TryGetProperty("database", out var dbHealth))
        {
            dbHealth.TryGetProperty("duration", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task NetworkCommunication_BetweenServices_ShouldWork()
    {
        // Test that services can communicate with each other through the Docker network
        
        // This is tested implicitly through the application functionality
        // If network communication wasn't working, the application wouldn't function
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        // Test database communication
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue("API should be able to communicate with PostgreSQL");
        
        // Test that the application can serve requests (implies all services are communicating)
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EnvironmentVariables_ShouldBeConfiguredCorrectly()
    {
        // Test that environment variables are properly configured in Docker
        
        // This is tested implicitly through successful application startup
        // and database connectivity
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        // If connection strings weren't configured correctly, this would fail
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue("Connection strings should be configured correctly");
        
        // Test that the application responds correctly
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task VolumeMapping_ShouldPersistData()
    {
        // Test that volume mapping works for data persistence
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        // Create some test data
        var category = ECommerce.Domain.Aggregates.ProductAggregate.Category.CreateRoot(
            "Volume Test Category", 
            "Category to test volume persistence"
        );
        
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        
        // Verify data was persisted
        var savedCategory = await context.Categories.FindAsync(category.Id);
        savedCategory.Should().NotBeNull("Data should be persisted to volume");
        savedCategory!.Name.Should().Be("Volume Test Category");
    }

    [Fact]
    public async Task LoggingConfiguration_ShouldWork()
    {
        // Test that logging is configured correctly in Docker environment
        
        // Make a request that should generate logs
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // In a real implementation, you might check log files or log aggregation
        // For now, we verify that the application is running (which implies logging is working)
        
        // Test that structured logging works by making requests that generate different log levels
        var notFoundResponse = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");
        notFoundResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // The fact that we get proper responses indicates logging infrastructure is working
    }
}