using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers.Orders;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Orders;

namespace ECommerce.Application.Tests.Handlers.Orders;

public class GetOrdersQueryHandlerTests
{
    private readonly Mock<IOrderQueryService> _orderQueryServiceMock;
    private readonly Mock<ILogger<GetOrdersQueryHandler>> _loggerMock;
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTests()
    {
        _orderQueryServiceMock = new Mock<IOrderQueryService>();
        _loggerMock = new Mock<ILogger<GetOrdersQueryHandler>>();
        _handler = new GetOrdersQueryHandler(
            _orderQueryServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenOrdersExist()
    {
        // Arrange
        var query = new GetOrdersQuery(
            SearchTerm: "test",
            Page: 1,
            PageSize: 10
        );

        var orderDtos = new List<OrderDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), new CustomerSummaryDto(Guid.NewGuid(), "John", "john@test.com", null),
                "Confirmed", "", "", [], null, 100, "USD", 1, DateTime.UtcNow, DateTime.UtcNow, null, null, null, null, null),
            new(Guid.NewGuid(), Guid.NewGuid(), new CustomerSummaryDto(Guid.NewGuid(), "Jane", "jane@test.com", null),
                "Shipped", "", "", [], null, 200, "USD", 2, DateTime.UtcNow, DateTime.UtcNow, null, null, null, null, null)
        };

        var expectedResult = new PagedResult<OrderDto>
        {
            Items = orderDtos,
            Page = 1,
            PageSize = 10,
            TotalCount = 2
        };

        _orderQueryServiceMock
            .Setup(x => x.GetOrdersAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCallOrderQueryService_WithCorrectQuery()
    {
        // Arrange
        var query = new GetOrdersQuery(
            SearchTerm: "order search",
            CustomerId: Guid.NewGuid(),
            Status: "Confirmed",
            Page: 1,
            PageSize: 25
        );

        var expectedResult = new PagedResult<OrderDto>
        {
            Items = [],
            Page = 1,
            PageSize = 25,
            TotalCount = 0
        };

        _orderQueryServiceMock
            .Setup(x => x.GetOrdersAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _orderQueryServiceMock.Verify(
            x => x.GetOrdersAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenSuccessful()
    {
        // Arrange
        var query = new GetOrdersQuery(SearchTerm: "test", Page: 1, PageSize: 10);

        var expectedResult = new PagedResult<OrderDto>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        _orderQueryServiceMock
            .Setup(x => x.GetOrdersAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling GetOrdersQuery")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenServiceThrows()
    {
        // Arrange
        var query = new GetOrdersQuery(SearchTerm: "test");
        var expectedException = new InvalidOperationException("Service error");

        _orderQueryServiceMock
            .Setup(x => x.GetOrdersAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Should().Be(expectedException);
    }
}