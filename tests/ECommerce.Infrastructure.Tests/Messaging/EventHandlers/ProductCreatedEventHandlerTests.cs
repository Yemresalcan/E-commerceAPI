using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;
using ECommerce.Infrastructure.Messaging.EventHandlers;
using ECommerce.ReadModel.Models;
using ECommerce.ReadModel.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Infrastructure.Tests.Messaging.EventHandlers;

/// <summary>
/// Integration tests for ProductCreatedEventHandler
/// </summary>
public class ProductCreatedEventHandlerTests
{
    private readonly Mock<ILogger<ProductCreatedEventHandler>> _loggerMock;
    private readonly Mock<IProductSearchService> _productSearchServiceMock;
    private readonly ProductCreatedEventHandler _handler;

    public ProductCreatedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ProductCreatedEventHandler>>();
        _productSearchServiceMock = new Mock<IProductSearchService>();
        var cacheInvalidationServiceMock = new Mock<ICacheInvalidationService>();
        _handler = new ProductCreatedEventHandler(_loggerMock.Object, _productSearchServiceMock.Object, cacheInvalidationServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_ShouldIndexProductInElasticsearch()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var domainEvent = new ProductCreatedEvent(
            productId,
            "Test Product",
            99.99m,
            "USD",
            categoryId,
            100
        );

        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _productSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<ProductReadModel>(p => 
                    p.Id == productId &&
                    p.Name == "Test Product" &&
                    p.Price == 99.99m &&
                    p.Currency == "USD" &&
                    p.StockQuantity == 100 &&
                    p.Category.Id == categoryId &&
                    p.IsActive == true &&
                    p.IsInStock == true &&
                    p.IsLowStock == false &&
                    p.IsOutOfStock == false
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_LowStockProduct_ShouldSetLowStockFlag()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var domainEvent = new ProductCreatedEvent(
            productId,
            "Low Stock Product",
            49.99m,
            "USD",
            categoryId,
            5 // Low stock quantity
        );

        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _productSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<ProductReadModel>(p => 
                    p.IsLowStock == true &&
                    p.IsInStock == true &&
                    p.IsOutOfStock == false
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_OutOfStockProduct_ShouldSetOutOfStockFlag()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var domainEvent = new ProductCreatedEvent(
            productId,
            "Out of Stock Product",
            29.99m,
            "USD",
            categoryId,
            0 // Out of stock
        );

        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _productSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<ProductReadModel>(p => 
                    p.IsOutOfStock == true &&
                    p.IsInStock == false &&
                    p.IsLowStock == true // When stock is 0, it's both out of stock and low stock
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_ElasticsearchIndexingFails_ShouldLogWarning()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var domainEvent = new ProductCreatedEvent(
            productId,
            "Test Product",
            99.99m,
            "USD",
            categoryId,
            100
        );

        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to index product {productId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_ElasticsearchThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var domainEvent = new ProductCreatedEvent(
            productId,
            "Test Product",
            99.99m,
            "USD",
            categoryId,
            100
        );

        var exception = new InvalidOperationException("Elasticsearch connection failed");
        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(domainEvent)
        );

        Assert.Equal(exception, thrownException);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error handling ProductCreatedEvent for product {productId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_ShouldSetCorrectSuggestField()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var productName = "Amazing Product";
        var domainEvent = new ProductCreatedEvent(
            productId,
            productName,
            199.99m,
            "EUR",
            categoryId,
            50
        );

        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _productSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<ProductReadModel>(p => 
                    p.Suggest.Input.Contains(productName)
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}