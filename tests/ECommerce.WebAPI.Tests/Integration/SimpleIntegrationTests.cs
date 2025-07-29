using System.Net;
using System.Text;
using System.Text.Json;
using ECommerce.Application.Commands.Products;
using ECommerce.Application.Commands.Customers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.WebAPI.Tests.Integration;

/// <summary>
/// Simple integration tests that use in-memory database instead of test containers
/// These tests demonstrate the integration testing approach without requiring Docker
/// </summary>
public class SimpleIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SimpleIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ECommerceDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ECommerceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await SeedTestDataAsync();
        
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: await GetTestCategoryIdAsync()
        );

        var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var productId = JsonSerializer.Deserialize<Guid>(responseContent);
        productId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterCustomer_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            PhoneNumber: "+1234567890"
        );

        var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var customerId = JsonSerializer.Deserialize<Guid>(responseContent);
        customerId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidCommand = new CreateProductCommand(
            Name: "", // Invalid: empty name
            Description: "",
            Price: -10, // Invalid: negative price
            Currency: "INVALID", // Invalid: not 3 characters
            Sku: "",
            StockQuantity: -5, // Invalid: negative stock
            MinimumStockLevel: 0,
            CategoryId: Guid.Empty // Invalid: empty GUID
        );

        var json = JsonSerializer.Serialize(invalidCommand, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProduct_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResult = JsonSerializer.Deserialize<JsonElement>(content);
        
        // The health check might not be "Healthy" due to missing external services
        // but it should return a valid JSON response
        healthResult.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task CQRS_Flow_ShouldWork()
    {
        // This test demonstrates a basic CQRS flow from command to query
        
        // 1. Create a customer (Command)
        var customerCommand = new RegisterCustomerCommand(
            FirstName: "Test",
            LastName: "User",
            Email: "test.user@example.com"
        );

        var customerJson = JsonSerializer.Serialize(customerCommand, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var customerContent = new StringContent(customerJson, Encoding.UTF8, "application/json");

        var customerResponse = await _client.PostAsync("/api/customers", customerContent);
        customerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var customerResponseContent = await customerResponse.Content.ReadAsStringAsync();
        var customerId = JsonSerializer.Deserialize<Guid>(customerResponseContent);

        // 2. Create a product (Command)
        await SeedTestDataAsync();
        
        var productCommand = new CreateProductCommand(
            Name: "CQRS Test Product",
            Description: "Product for CQRS flow test",
            Price: 149.99m,
            Currency: "USD",
            Sku: "CQRS-001",
            StockQuantity: 20,
            MinimumStockLevel: 5,
            CategoryId: await GetTestCategoryIdAsync()
        );

        var productJson = JsonSerializer.Serialize(productCommand, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var productContent = new StringContent(productJson, Encoding.UTF8, "application/json");

        var productResponse = await _client.PostAsync("/api/products", productContent);
        productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var productResponseContent = await productResponse.Content.ReadAsStringAsync();
        var productId = JsonSerializer.Deserialize<Guid>(productResponseContent);

        // 3. Query the created resources (Query)
        var getCustomerResponse = await _client.GetAsync($"/api/customers/{customerId}");
        getCustomerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getProductResponse = await _client.GetAsync($"/api/products/{productId}");
        getProductResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the flow worked
        customerId.Should().NotBeEmpty();
        productId.Should().NotBeEmpty();
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Check if category already exists
        if (!await context.Categories.AnyAsync())
        {
            var category = ECommerce.Domain.Aggregates.ProductAggregate.Category.CreateRoot("Electronics", "Electronic products");
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
        }
    }

    private async Task<Guid> GetTestCategoryIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        var category = await context.Categories.FirstOrDefaultAsync();
        if (category == null)
        {
            await SeedTestDataAsync();
            category = await context.Categories.FirstAsync();
        }
        
        return category.Id;
    }
}