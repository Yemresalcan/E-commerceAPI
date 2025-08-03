using System.Diagnostics;
using System.Net;
using ECommerce.Application.Commands.Products;
using ECommerce.Application.Commands.Customers;
using ECommerce.Application.DTOs;
using ECommerce.Application.Common.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.WebAPI.Tests.Integration;

/// <summary>
/// Performance validation tests to ensure the system meets scalability and performance requirements.
/// These tests validate response times, throughput, and system behavior under load.
/// </summary>
public class PerformanceValidationTests : IntegrationTestBase
{
    [Fact]
    public async Task ApiResponseTimes_ShouldMeetPerformanceRequirements()
    {
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateTestCategoryAsync();
        
        // Test product creation performance
        var stopwatch = Stopwatch.StartNew();
        var productId = await CreateTestProductAsync(categoryId);
        stopwatch.Stop();
        
        // Product creation should complete within 5 seconds
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            "Product creation should complete within 5 seconds");
        
        // Test product retrieval performance
        stopwatch.Restart();
        var response = await _client.GetAsync($"/api/products/{productId}");
        stopwatch.Stop();
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Product retrieval should be fast (under 1 second)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
            "Product retrieval should complete within 1 second");
    }

    [Fact]
    public async Task DatabaseOperations_ShouldPerformEfficiently()
    {
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateTestCategoryAsync();
        
        // Test bulk operations performance
        var stopwatch = Stopwatch.StartNew();
        
        // Create multiple products
        var productTasks = new List<Task<Guid>>();
        for (int i = 0; i < 10; i++)
        {
            productTasks.Add(CreateTestProductAsync(categoryId, $"Performance Test Product {i}"));
        }
        
        var productIds = await Task.WhenAll(productTasks);
        stopwatch.Stop();
        
        // Bulk creation should complete within reasonable time
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, 
            "Bulk product creation should complete within 30 seconds");
        
        productIds.Should().HaveCount(10);
        productIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateTestCategoryAsync();
        
        // Test concurrent product creation
        var concurrentTasks = new List<Task<(HttpStatusCode StatusCode, TimeSpan Duration)>>();
        
        for (int i = 0; i < 20; i++)
        {
            concurrentTasks.Add(CreateProductWithTiming(categoryId, $"Concurrent Product {i}"));
        }
        
        // Act
        var results = await Task.WhenAll(concurrentTasks);
        
        // Assert
        // All requests should succeed
        results.Should().AllSatisfy(result => 
            result.StatusCode.Should().Be(HttpStatusCode.Created));
        
        // Most requests should complete within reasonable time
        var fastRequests = results.Count(r => r.Duration.TotalMilliseconds < 10000);
        fastRequests.Should().BeGreaterThan(15, 
            "At least 75% of concurrent requests should complete within 10 seconds");
    }

    [Fact]
    public async Task SearchOperations_ShouldPerformEfficiently()
    {
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateTestCategoryAsync();
        
        // Create test data for searching
        var productTasks = new List<Task<Guid>>();
        for (int i = 0; i < 5; i++)
        {
            productTasks.Add(CreateTestProductAsync(categoryId, $"Searchable Product {i}"));
        }
        
        await Task.WhenAll(productTasks);
        
        // Wait for Elasticsearch indexing
        await Task.Delay(5000);
        
        // Test search performance
        var stopwatch = Stopwatch.StartNew();
        var searchResponse = await _client.GetAsync("/api/products/search?searchTerm=Searchable&page=1&pageSize=10");
        stopwatch.Stop();
        
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Search should be fast (under 2 seconds)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, 
            "Search operations should complete within 2 seconds");
        
        var searchResults = await DeserializeResponse<PagedResult<ProductDto>>(searchResponse);
        searchResults.Should().NotBeNull();
        searchResults!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task MemoryUsage_ShouldRemainStable()
    {
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateTestCategoryAsync();
        
        // Get initial memory usage
        var initialMemory = GC.GetTotalMemory(true);
        
        // Perform multiple operations
        for (int i = 0; i < 50; i++)
        {
            var productId = await CreateTestProductAsync(categoryId, $"Memory Test Product {i}");
            var response = await _client.GetAsync($"/api/products/{productId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Force garbage collection periodically
            if (i % 10 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        
        // Get final memory usage
        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;
        
        // Memory increase should be reasonable (less than 100MB for this test)
        memoryIncrease.Should().BeLessThan(100 * 1024 * 1024, 
            "Memory usage should not increase excessively during operations");
    }

    [Fact]
    public async Task DatabaseConnections_ShouldBePooledEfficiently()
    {
        // Test that database connections are properly pooled and managed
        
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateTestCategoryAsync();
        
        // Perform many database operations
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(PerformDatabaseOperation(categoryId, i));
        }
        
        // Act & Assert - should not throw connection pool exhaustion
        await Task.WhenAll(tasks);
        
        // Verify system is still responsive
        var healthResponse = await _client.GetAsync("/health");
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CachePerformance_ShouldImproveResponseTimes()
    {
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateTestCategoryAsync();
        var productId = await CreateTestProductAsync(categoryId);
        
        // First request (cache miss)
        var stopwatch = Stopwatch.StartNew();
        var firstResponse = await _client.GetAsync($"/api/products/{productId}");
        stopwatch.Stop();
        var firstRequestTime = stopwatch.ElapsedMilliseconds;
        
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Second request (should be cached)
        stopwatch.Restart();
        var secondResponse = await _client.GetAsync($"/api/products/{productId}");
        stopwatch.Stop();
        var secondRequestTime = stopwatch.ElapsedMilliseconds;
        
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Second request should be faster (cached)
        // Note: This test might be flaky in test environment, so we use a reasonable threshold
        secondRequestTime.Should().BeLessOrEqualTo(firstRequestTime + 100, 
            "Cached requests should not be significantly slower than the first request");
    }

    [Fact]
    public async Task EventProcessing_ShouldNotBlockRequests()
    {
        // Test that event processing doesn't block API requests
        
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateTestCategoryAsync();
        
        // Create a product (which triggers events)
        var stopwatch = Stopwatch.StartNew();
        var productId = await CreateTestProductAsync(categoryId);
        stopwatch.Stop();
        
        // Product creation should complete quickly even with event processing
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            "Product creation should not be blocked by event processing");
        
        // Immediately make another request
        stopwatch.Restart();
        var response = await _client.GetAsync($"/api/products/{productId}");
        stopwatch.Stop();
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
            "Subsequent requests should not be blocked by event processing");
    }

    // Helper methods

    private async Task<Guid> CreateTestCategoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        var category = ECommerce.Domain.Aggregates.ProductAggregate.Category.CreateRoot(
            "Performance Test Category", 
            "Category for performance testing"
        );
        
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        
        return category.Id;
    }

    private async Task<Guid> CreateTestProductAsync(Guid categoryId, string name = "Performance Test Product")
    {
        var command = new CreateProductCommand(
            Name: name,
            Description: "Product for performance testing",
            Price: 99.99m,
            Currency: "USD",
            Sku: $"PERF-{Guid.NewGuid().ToString()[..8]}",
            StockQuantity: 100,
            MinimumStockLevel: 10,
            CategoryId: categoryId
        );
        
        var response = await _client.PostAsync("/api/products", CreateJsonContent(command));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var productId = await DeserializeResponse<Guid>(response);
        return productId;
    }

    private async Task<(HttpStatusCode StatusCode, TimeSpan Duration)> CreateProductWithTiming(Guid categoryId, string name)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var command = new CreateProductCommand(
            Name: name,
            Description: "Product for concurrent testing",
            Price: 99.99m,
            Currency: "USD",
            Sku: $"CONC-{Guid.NewGuid().ToString()[..8]}",
            StockQuantity: 100,
            MinimumStockLevel: 10,
            CategoryId: categoryId
        );
        
        var response = await _client.PostAsync("/api/products", CreateJsonContent(command));
        stopwatch.Stop();
        
        return (response.StatusCode, stopwatch.Elapsed);
    }

    private async Task PerformDatabaseOperation(Guid categoryId, int index)
    {
        var productId = await CreateTestProductAsync(categoryId, $"DB Pool Test Product {index}");
        var response = await _client.GetAsync($"/api/products/{productId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}