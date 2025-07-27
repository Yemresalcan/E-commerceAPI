using ECommerce.Domain.Events;
using ECommerce.Infrastructure.Messaging.EventHandlers;
using ECommerce.ReadModel.Models;
using ECommerce.ReadModel.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Infrastructure.Tests.Messaging.EventHandlers;

/// <summary>
/// Integration tests for event handlers working together
/// </summary>
public class EventHandlersIntegrationTests
{
    private readonly Mock<ILogger<ProductCreatedEventHandler>> _productLoggerMock;
    private readonly Mock<ILogger<OrderPlacedEventHandler>> _orderLoggerMock;
    private readonly Mock<ILogger<CustomerRegisteredEventHandler>> _customerLoggerMock;
    private readonly Mock<IProductSearchService> _productSearchServiceMock;
    private readonly Mock<IOrderSearchService> _orderSearchServiceMock;
    private readonly Mock<ICustomerSearchService> _customerSearchServiceMock;

    public EventHandlersIntegrationTests()
    {
        _productLoggerMock = new Mock<ILogger<ProductCreatedEventHandler>>();
        _orderLoggerMock = new Mock<ILogger<OrderPlacedEventHandler>>();
        _customerLoggerMock = new Mock<ILogger<CustomerRegisteredEventHandler>>();
        _productSearchServiceMock = new Mock<IProductSearchService>();
        _orderSearchServiceMock = new Mock<IOrderSearchService>();
        _customerSearchServiceMock = new Mock<ICustomerSearchService>();
    }

    [Fact]
    public async Task CompleteECommerceFlow_ShouldUpdateAllReadModels()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var customerHandler = new CustomerRegisteredEventHandler(_customerLoggerMock.Object, _customerSearchServiceMock.Object);
        var productHandler = new ProductCreatedEventHandler(_productLoggerMock.Object, _productSearchServiceMock.Object);
        var orderHandler = new OrderPlacedEventHandler(_orderLoggerMock.Object, _orderSearchServiceMock.Object);

        // Setup all services to return success
        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _orderSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Create domain events
        var customerRegisteredEvent = new CustomerRegisteredEvent(
            customerId,
            "john.doe@example.com",
            "John",
            "Doe",
            "+1234567890"
        );

        var productCreatedEvent = new ProductCreatedEvent(
            productId,
            "Amazing Product",
            99.99m,
            "USD",
            categoryId,
            50
        );

        var orderPlacedEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            199.98m,
            "USD",
            2,
            "123 Main St, City, State 12345"
        );

        // Act - Simulate the complete e-commerce flow
        await customerHandler.HandleAsync(customerRegisteredEvent);
        await productHandler.HandleAsync(productCreatedEvent);
        await orderHandler.HandleAsync(orderPlacedEvent);

        // Assert - Verify all read models were updated
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<CustomerReadModel>(c => 
                    c.Id == customerId &&
                    c.Email == "john.doe@example.com" &&
                    c.FullName == "John Doe"
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _productSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<ProductReadModel>(p => 
                    p.Id == productId &&
                    p.Name == "Amazing Product" &&
                    p.Price == 99.99m
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _orderSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<OrderReadModel>(o => 
                    o.Id == orderId &&
                    o.CustomerId == customerId &&
                    o.TotalAmount == 199.98m
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task EventHandlers_WhenElasticsearchFails_ShouldLogWarningsButNotThrow()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var customerHandler = new CustomerRegisteredEventHandler(_customerLoggerMock.Object, _customerSearchServiceMock.Object);
        var productHandler = new ProductCreatedEventHandler(_productLoggerMock.Object, _productSearchServiceMock.Object);
        var orderHandler = new OrderPlacedEventHandler(_orderLoggerMock.Object, _orderSearchServiceMock.Object);

        // Setup all services to return failure
        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _orderSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Create domain events
        var customerRegisteredEvent = new CustomerRegisteredEvent(customerId, "test@example.com", "Test", "User");
        var productCreatedEvent = new ProductCreatedEvent(productId, "Test Product", 50.00m, "USD", categoryId, 10);
        var orderPlacedEvent = new OrderPlacedEvent(orderId, customerId, 50.00m, "USD", 1, "Test Address");

        // Act - Should not throw exceptions
        await customerHandler.HandleAsync(customerRegisteredEvent);
        await productHandler.HandleAsync(productCreatedEvent);
        await orderHandler.HandleAsync(orderPlacedEvent);

        // Assert - Verify warning logs were written
        _customerLoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to index customer {customerId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        _productLoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to index product {productId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );

        _orderLoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to index order {orderId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task EventHandlers_WithConcurrentEvents_ShouldHandleAllEvents()
    {
        // Arrange
        var customerHandler = new CustomerRegisteredEventHandler(_customerLoggerMock.Object, _customerSearchServiceMock.Object);
        var productHandler = new ProductCreatedEventHandler(_productLoggerMock.Object, _productSearchServiceMock.Object);
        var orderHandler = new OrderPlacedEventHandler(_orderLoggerMock.Object, _orderSearchServiceMock.Object);

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _orderSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Create multiple events
        var customerEvents = Enumerable.Range(1, 5)
            .Select(i => new CustomerRegisteredEvent(Guid.NewGuid(), $"user{i}@example.com", $"User{i}", "Test"))
            .ToList();

        var productEvents = Enumerable.Range(1, 5)
            .Select(i => new ProductCreatedEvent(Guid.NewGuid(), $"Product {i}", i * 10.0m, "USD", Guid.NewGuid(), i * 10))
            .ToList();

        var orderEvents = Enumerable.Range(1, 5)
            .Select(i => new OrderPlacedEvent(Guid.NewGuid(), Guid.NewGuid(), i * 25.0m, "USD", i, $"Address {i}"))
            .ToList();

        // Act - Handle all events concurrently
        var customerTasks = customerEvents.Select(e => customerHandler.HandleAsync(e));
        var productTasks = productEvents.Select(e => productHandler.HandleAsync(e));
        var orderTasks = orderEvents.Select(e => orderHandler.HandleAsync(e));

        await Task.WhenAll(customerTasks.Concat(productTasks).Concat(orderTasks));

        // Assert - Verify all events were processed
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()),
            Times.Exactly(5)
        );

        _productSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()),
            Times.Exactly(5)
        );

        _orderSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()),
            Times.Exactly(5)
        );
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    public async Task EventHandlers_WithDifferentCurrencies_ShouldPreserveCurrencyInformation(string currency)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var customerHandler = new CustomerRegisteredEventHandler(_customerLoggerMock.Object, _customerSearchServiceMock.Object);
        var productHandler = new ProductCreatedEventHandler(_productLoggerMock.Object, _productSearchServiceMock.Object);
        var orderHandler = new OrderPlacedEventHandler(_orderLoggerMock.Object, _orderSearchServiceMock.Object);

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _productSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<ProductReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _orderSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Create events with specific currency
        var customerRegisteredEvent = new CustomerRegisteredEvent(customerId, "currency@example.com", "Currency", "Test");
        var productCreatedEvent = new ProductCreatedEvent(productId, "Currency Product", 100.0m, currency, categoryId, 20);
        var orderPlacedEvent = new OrderPlacedEvent(orderId, customerId, 100.0m, currency, 1, "Currency Address");

        // Act
        await customerHandler.HandleAsync(customerRegisteredEvent);
        await productHandler.HandleAsync(productCreatedEvent);
        await orderHandler.HandleAsync(orderPlacedEvent);

        // Assert - Verify currency is preserved
        _productSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<ProductReadModel>(p => p.Currency == currency),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _orderSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<OrderReadModel>(o => o.Currency == currency),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}