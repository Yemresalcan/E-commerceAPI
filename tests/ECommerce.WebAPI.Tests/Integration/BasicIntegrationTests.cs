using System.Net;
using System.Text;
using System.Text.Json;
using ECommerce.Application.Commands.Products;
using ECommerce.Application.Commands.Customers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.WebAPI.Tests.Integration;

public class BasicIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        // First create a category in the database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerce.Infrastructure.Persistence.ECommerceDbContext>();
        
        var category = ECommerce.Domain.Aggregates.ProductAggregate.Category.CreateRoot("Electronics", "Electronic products");
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: category.Id
        );

        // Act
        var response = await _client.PostAsync("/api/products", CreateJsonContent(command));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var productId = await DeserializeResponse<Guid>(response);
        productId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterCustomer_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var command = new RegisterCustomerCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            PhoneNumber: "+1234567890"
        );

        // Act
        var response = await _client.PostAsync("/api/customers", CreateJsonContent(command));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var customerId = await DeserializeResponse<Guid>(response);
        customerId.Should().NotBeEmpty();
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
        
        healthResult.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task ValidationPipeline_ShouldReturnValidationErrors()
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

        // Act
        var response = await _client.PostAsync("/api/products", CreateJsonContent(invalidCommand));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetNonExistentResource_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DatabaseConnectivity_ShouldWork()
    {
        // This test verifies that the database connection is working
        // by directly accessing the database context
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerce.Infrastructure.Persistence.ECommerceDbContext>();
        
        // Act & Assert - should not throw
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }
}