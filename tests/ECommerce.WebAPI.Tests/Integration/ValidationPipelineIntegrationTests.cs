using System.Net;
using System.Text;
using System.Text.Json;
using ECommerce.Application.Commands.Products;
using ECommerce.WebAPI.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ECommerce.WebAPI.Tests.Integration;

public class ValidationPipelineIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ValidationPipelineIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsValidationError()
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ValidationErrorResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Validation Failed", errorResponse.Title);
        Assert.NotNull(errorResponse.Errors);
        Assert.True(errorResponse.Errors.Count > 0, "Should have validation errors");
        
        // Check for specific validation errors
        Assert.Contains("Name", errorResponse.Errors.Keys);
        Assert.Contains("Price", errorResponse.Errors.Keys);
        Assert.Contains("Currency", errorResponse.Errors.Keys);
    }

    [Fact]
    public async Task GetProduct_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/products/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(errorResponse);
        Assert.Contains("not found", errorResponse.Detail.ToLower());
    }
}