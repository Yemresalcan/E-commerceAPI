using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers.Orders;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Orders;

namespace ECommerce.Application.Tests.Handlers.Orders;

public class GetOrderQueryHandlerTests
{
    private readonly Mock<IOrderQueryService> _orderQueryServiceMock;
    private readonly Mock<ILogger<GetOrderQueryHandler>> _loggerMock;
    private readonly GetOrderQueryHandler _handler;

    public GetOrderQueryHandlerTests()
    {
        _orderQueryServiceMock = new Mock<IOrderQueryService>();
        _loggerMock = new Mock<ILogger<GetOrderQueryHandler>>();
        _handler = new GetOrderQueryHandler(
            _orderQueryServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderDto_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        var orderDto = new OrderDto(
            orderId,
            Guid.NewGuid(),
            new CustomerSummaryDto(Guid.NewGuid(), "John Doe", "john@example.com", "+1234567890"),
            "Confirmed",
            "123 Main St",
            "123 Main St",
            [],
            null,
            150.00m,
            "USD",
            2,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null,
            null,
            null
        );

        _orderQueryServiceMock
            .Setup(x => x.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.Status.Should().Be("Confirmed");
        result.TotalAmount.Should().Be(150.00m);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        _orderQueryServiceMock
            .Setup(x => x.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallOrderQueryService_WithCorrectId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        _orderQueryServiceMock
            .Setup(x => x.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _orderQueryServiceMock.Verify(
            x => x.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenOrderFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        var orderDto = new OrderDto(
            orderId, Guid.NewGuid(), new CustomerSummaryDto(Guid.NewGuid(), "John", "john@test.com", null),
            "Confirmed", "", "", [], null, 100, "USD", 1, DateTime.UtcNow, DateTime.UtcNow,
            null, null, null, null, null
        );

        _orderQueryServiceMock
            .Setup(x => x.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling GetOrderQuery")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved order")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        _orderQueryServiceMock
            .Setup(x => x.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenServiceFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);
        var expectedException = new InvalidOperationException("Service error");

        _orderQueryServiceMock
            .Setup(x => x.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Should().Be(expectedException);
    }
}