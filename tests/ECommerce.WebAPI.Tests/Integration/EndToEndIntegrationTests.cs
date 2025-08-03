using System.Net;
using System.Text.Json;
using ECommerce.Application.Commands.Products;
using ECommerce.Application.Commands.Customers;
using ECommerce.Application.Commands.Orders;
using ECommerce.Application.DTOs;
using ECommerce.Application.Common.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.WebAPI.Tests.Integration;

/// <summary>
/// Comprehensive end-to-end integration tests that validate the complete CQRS flows,
/// Docker compose setup, and performance requirements for the e-commerce system.
/// </summary>
public class EndToEndIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CompleteECommerceWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange - Clear database and prepare test data
        await ClearDatabaseAsync();
        
        // Step 1: Create Category (prerequisite for products)
        var categoryId = await CreateCategoryAsync();
        
        // Step 2: Create Product via Command (Write side)
        var productId = await CreateProductAsync(categoryId);
        
        // Step 3: Register Customer via Command (Write side)
        var customerId = await RegisterCustomerAsync();
        
        // Step 4: Add Customer Address
        var addressId = await AddCustomerAddressAsync(customerId);
        
        // Step 5: Place Order via Command (Write side)
        var orderId = await PlaceOrderAsync(customerId, productId, addressId);
        
        // Step 6: Verify Read Models are Updated (Read side)
        await VerifyReadModelsAsync(productId, customerId, orderId);
        
        // Step 7: Test Search Functionality (Elasticsearch)
        await VerifySearchFunctionalityAsync();
        
        // Step 8: Test Order Status Updates
        await UpdateOrderStatusAsync(orderId);
        
        // Step 9: Verify Event Processing and Read Model Consistency
        await VerifyEventProcessingAsync(orderId);
    }

    [Fact]
    public async Task CQRSFlows_ShouldMaintainEventualConsistency()
    {
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateCategoryAsync();
        
        // Test Command -> Event -> Read Model Update flow
        var productId = await CreateProductAsync(categoryId);
        
        // Wait for event processing (eventual consistency)
        await Task.Delay(2000);
        
        // Verify write model
        var writeModelResponse = await _client.GetAsync($"/api/products/{productId}");
        writeModelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify read model (should be eventually consistent)
        var searchResponse = await _client.GetAsync("/api/products/search?searchTerm=Integration");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var searchResults = await DeserializeResponse<PagedResult<ProductDto>>(searchResponse);
        searchResults.Should().NotBeNull();
        searchResults!.Items.Should().ContainSingle(p => p.Id == productId);
    }

    [Fact]
    public async Task HealthChecks_AllServices_ShouldBeHealthy()
    {
        // Test all health check endpoints
        var healthResponse = await _client.GetAsync("/health");
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(healthContent);
        
        // Verify overall health status
        healthResult.GetProperty("status").GetString().Should().Be("Healthy");
        
        // Verify individual service health checks
        var entries = healthResult.GetProperty("entries");
        
        // PostgreSQL health check
        entries.GetProperty("database").GetProperty("status").GetString().Should().Be("Healthy");
        
        // Redis health check (if configured)
        if (entries.TryGetProperty("redis", out var redisHealth))
        {
            redisHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
        
        // Elasticsearch health check (if configured)
        if (entries.TryGetProperty("elasticsearch", out var esHealth))
        {
            esHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
        
        // RabbitMQ health check (if configured)
        if (entries.TryGetProperty("rabbitmq", out var rabbitHealth))
        {
            rabbitHealth.GetProperty("status").GetString().Should().Be("Healthy");
        }
    }

    [Fact]
    public async Task PerformanceValidation_ShouldMeetRequirements()
    {
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateCategoryAsync();
        
        // Test API response times
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Create product (should complete within reasonable time)
        var productId = await CreateProductAsync(categoryId);
        stopwatch.Stop();
        
        // Verify performance requirement (should be under 5 seconds for creation)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
        
        // Test query performance
        stopwatch.Restart();
        var response = await _client.GetAsync($"/api/products/{productId}");
        stopwatch.Stop();
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Query should be fast (under 1 second)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldHandleCorrectly()
    {
        // Arrange
        await ClearDatabaseAsync();
        var categoryId = await CreateCategoryAsync();
        
        // Test concurrent product creation
        var tasks = new List<Task<Guid>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(CreateProductAsync(categoryId, $"Concurrent Product {i}"));
        }
        
        // Act
        var productIds = await Task.WhenAll(tasks);
        
        // Assert
        productIds.Should().HaveCount(5);
        productIds.Should().OnlyHaveUniqueItems();
        
        // Verify all products were created
        foreach (var productId in productIds)
        {
            var response = await _client.GetAsync($"/api/products/{productId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task ErrorHandling_ShouldReturnAppropriateResponses()
    {
        // Test validation errors
        var invalidCommand = new CreateProductCommand(
            Name: "", // Invalid
            Description: "Test",
            Price: -10, // Invalid
            Currency: "USD",
            Sku: "",
            StockQuantity: -5, // Invalid
            MinimumStockLevel: 0,
            CategoryId: Guid.Empty // Invalid
        );
        
        var response = await _client.PostAsync("/api/products", CreateJsonContent(invalidCommand));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Test not found scenarios
        var notFoundResponse = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");
        notFoundResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        // Test business rule violations
        var customerId = await RegisterCustomerAsync();
        var categoryId = await CreateCategoryAsync();
        var productId = await CreateProductAsync(categoryId);
        
        // Try to place order with invalid quantity
        var invalidOrderCommand = new PlaceOrderCommand(
            CustomerId: customerId,
            ShippingAddress: "123 Test St, Test City, TS 12345, Test Country",
            BillingAddress: "123 Test St, Test City, TS 12345, Test Country",
            OrderItems: new List<OrderItemDto>
            {
                new(ProductId: productId, ProductName: "Test Product", Quantity: 1000, UnitPrice: 99.99m, Currency: "USD") // Exceeds stock
            }
        );
        
        var orderResponse = await _client.PostAsync("/api/orders", CreateJsonContent(invalidOrderCommand));
        orderResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Helper methods for test scenarios

    private async Task<Guid> CreateCategoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        var category = Category.CreateRoot("Integration Test Category", "Category for integration tests");
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        
        return category.Id;
    }

    private async Task<Guid> CreateProductAsync(Guid categoryId, string name = "Integration Test Product")
    {
        var command = new CreateProductCommand(
            Name: name,
            Description: "Product for integration testing",
            Price: 99.99m,
            Currency: "USD",
            Sku: $"INT-{Guid.NewGuid().ToString()[..8]}",
            StockQuantity: 100,
            MinimumStockLevel: 10,
            CategoryId: categoryId
        );
        
        var response = await _client.PostAsync("/api/products", CreateJsonContent(command));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var productId = await DeserializeResponse<Guid>(response);
        return productId;
    }

    private async Task<Guid> RegisterCustomerAsync()
    {
        var command = new RegisterCustomerCommand(
            FirstName: "Integration",
            LastName: "Test",
            Email: $"integration.test.{Guid.NewGuid()}@example.com",
            PhoneNumber: "+1234567890"
        );
        
        var response = await _client.PostAsync("/api/customers", CreateJsonContent(command));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var customerId = await DeserializeResponse<Guid>(response);
        return customerId;
    }

    private async Task<Guid> AddCustomerAddressAsync(Guid customerId)
    {
        var command = new AddCustomerAddressCommand(
            CustomerId: customerId,
            Type: ECommerce.Domain.Aggregates.CustomerAggregate.AddressType.Shipping,
            Street1: "123 Integration Test St",
            City: "Test City",
            State: "TS",
            PostalCode: "12345",
            Country: "Test Country",
            Street2: null,
            Label: "Home",
            IsPrimary: true
        );
        
        var response = await _client.PostAsync($"/api/customers/{customerId}/addresses", CreateJsonContent(command));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var addressId = await DeserializeResponse<Guid>(response);
        return addressId;
    }

    private async Task<Guid> PlaceOrderAsync(Guid customerId, Guid productId, Guid addressId)
    {
        var command = new PlaceOrderCommand(
            CustomerId: customerId,
            ShippingAddress: "123 Integration Test St, Test City, TS 12345, Test Country",
            BillingAddress: "123 Integration Test St, Test City, TS 12345, Test Country",
            OrderItems: new List<OrderItemDto>
            {
                new(ProductId: productId, ProductName: "Integration Test Product", Quantity: 2, UnitPrice: 99.99m, Currency: "USD")
            }
        );
        
        var response = await _client.PostAsync("/api/orders", CreateJsonContent(command));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var orderId = await DeserializeResponse<Guid>(response);
        return orderId;
    }

    private async Task VerifyReadModelsAsync(Guid productId, Guid customerId, Guid orderId)
    {
        // Wait for eventual consistency
        await Task.Delay(2000);
        
        // Verify product read model
        var productResponse = await _client.GetAsync($"/api/products/{productId}");
        productResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify customer read model
        var customerResponse = await _client.GetAsync($"/api/customers/{customerId}");
        customerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify order read model
        var orderResponse = await _client.GetAsync($"/api/orders/{orderId}");
        orderResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task VerifySearchFunctionalityAsync()
    {
        // Wait for Elasticsearch indexing
        await Task.Delay(3000);
        
        // Test product search
        var searchResponse = await _client.GetAsync("/api/products/search?searchTerm=Integration");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var searchResults = await DeserializeResponse<PagedResult<ProductDto>>(searchResponse);
        searchResults.Should().NotBeNull();
        searchResults!.Items.Should().NotBeEmpty();
    }

    private async Task UpdateOrderStatusAsync(Guid orderId)
    {
        var command = new UpdateOrderStatusCommand(
            OrderId: orderId,
            NewStatus: ECommerce.Domain.Aggregates.OrderAggregate.OrderStatus.Confirmed,
            Reason: "Integration test status update"
        );
        
        var response = await _client.PutAsync($"/api/orders/{orderId}/status", CreateJsonContent(command));
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify status update
        var orderResponse = await _client.GetAsync($"/api/orders/{orderId}");
        orderResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var order = await DeserializeResponse<OrderDto>(orderResponse);
        order.Should().NotBeNull();
        order!.Status.Should().Be("Confirmed");
    }

    private async Task VerifyEventProcessingAsync(Guid orderId)
    {
        // Wait for event processing
        await Task.Delay(2000);
        
        // Verify that events were processed by checking read models
        var orderResponse = await _client.GetAsync($"/api/orders/{orderId}");
        orderResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Additional verification could include checking event store or audit logs
        // This would depend on the specific event handling implementation
    }
}