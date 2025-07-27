using ECommerce.Domain.Events;
using ECommerce.Infrastructure.Messaging.EventHandlers;
using ECommerce.ReadModel.Models;
using ECommerce.ReadModel.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Infrastructure.Tests.Messaging.EventHandlers;

/// <summary>
/// Integration tests for OrderPlacedEventHandler
/// </summary>
public class OrderPlacedEventHandlerTests
{
    private readonly Mock<ILogger<OrderPlacedEventHandler>> _loggerMock;
    private readonly Mock<IOrderSearchService> _orderSearchServiceMock;
    private readonly OrderPlacedEventHandler _handler;

    public OrderPlacedEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<OrderPlacedEventHandler>>();
        _orderSearchServiceMock = new Mock<IOrderSearchService>();
        _handler = new OrderPlacedEventHandler(_loggerMock.Object, _orderSearchServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_ShouldIndexOrderInElasticsearch()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var shippingAddress = "123 Main St, City, State 12345";
        var domainEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            299.99m,
            "USD",
            3,
            shippingAddress
        );

        _orderSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _orderSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<OrderReadModel>(o => 
                    o.Id == orderId &&
                    o.CustomerId == customerId &&
                    o.Customer.Id == customerId &&
                    o.Status == "Placed" &&
                    o.ShippingAddress == shippingAddress &&
                    o.BillingAddress == shippingAddress &&
                    o.TotalAmount == 299.99m &&
                    o.Currency == "USD" &&
                    o.TotalItemCount == 3 &&
                    o.ConfirmedAt == null &&
                    o.ShippedAt == null &&
                    o.DeliveredAt == null &&
                    o.CancelledAt == null
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_ShouldSetCorrectTimestamps()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var domainEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            149.99m,
            "EUR",
            2,
            "456 Oak Ave, Town, Country"
        );

        _orderSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _orderSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<OrderReadModel>(o => 
                    o.CreatedAt == domainEvent.OccurredOn &&
                    o.UpdatedAt == domainEvent.OccurredOn
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
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var domainEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            99.99m,
            "USD",
            1,
            "789 Pine St, Village, State"
        );

        _orderSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _loggerMock.Verify(
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
    public async Task HandleAsync_ElasticsearchThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var domainEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            199.99m,
            "GBP",
            4,
            "321 Elm St, City, Country"
        );

        var exception = new InvalidOperationException("Elasticsearch connection failed");
        _orderSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error handling OrderPlacedEvent for order {orderId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_ZeroItemCount_ShouldStillIndexOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var domainEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            0m,
            "USD",
            0,
            "Empty Order Address"
        );

        _orderSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _orderSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<OrderReadModel>(o => 
                    o.TotalAmount == 0m &&
                    o.TotalItemCount == 0
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_DifferentCurrencies_ShouldPreserveCurrency()
    {
        // Arrange
        var testCases = new[]
        {
            ("USD", 100.00m),
            ("EUR", 85.50m),
            ("GBP", 75.25m),
            ("JPY", 11000m)
        };

        foreach (var (currency, amount) in testCases)
        {
            var orderId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var domainEvent = new OrderPlacedEvent(
                orderId,
                customerId,
                amount,
                currency,
                2,
                "Test Address"
            );

            _orderSearchServiceMock
                .Setup(x => x.IndexDocumentAsync(It.IsAny<OrderReadModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await _handler.HandleAsync(domainEvent);

            // Assert
            _orderSearchServiceMock.Verify(
                x => x.IndexDocumentAsync(
                    It.Is<OrderReadModel>(o => 
                        o.Currency == currency &&
                        o.TotalAmount == amount
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }
    }
}