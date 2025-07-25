using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers.Customers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Customers;

namespace ECommerce.Application.Tests.Handlers.Customers;

public class GetCustomersQueryHandlerTests
{
    private readonly Mock<ICustomerQueryService> _customerQueryServiceMock;
    private readonly Mock<ILogger<GetCustomersQueryHandler>> _loggerMock;
    private readonly GetCustomersQueryHandler _handler;

    public GetCustomersQueryHandlerTests()
    {
        _customerQueryServiceMock = new Mock<ICustomerQueryService>();
        _loggerMock = new Mock<ILogger<GetCustomersQueryHandler>>();
        _handler = new GetCustomersQueryHandler(
            _customerQueryServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenCustomersExist()
    {
        // Arrange
        var query = new GetCustomersQuery(
            SearchTerm: "john",
            Page: 1,
            PageSize: 10
        );

        var customerDtos = new List<CustomerDto>
        {
            new(Guid.NewGuid(), "John", "Doe", "John Doe", "john@example.com", null, true, DateTime.UtcNow, null, [],
                new ProfileDto(null, null, "en", "USD", false, false, []),
                new CustomerStatisticsDto(0, 0, "USD", 0, null, null, 0, "New"),
                DateTime.UtcNow, DateTime.UtcNow),
            new(Guid.NewGuid(), "Jane", "Smith", "Jane Smith", "jane@example.com", null, true, DateTime.UtcNow, null, [],
                new ProfileDto(null, null, "en", "USD", false, false, []),
                new CustomerStatisticsDto(0, 0, "USD", 0, null, null, 0, "New"),
                DateTime.UtcNow, DateTime.UtcNow)
        };

        var expectedResult = new PagedResult<CustomerDto>
        {
            Items = customerDtos,
            Page = 1,
            PageSize = 10,
            TotalCount = 2
        };

        _customerQueryServiceMock
            .Setup(x => x.GetCustomersAsync(query, It.IsAny<CancellationToken>()))
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
    public async Task Handle_ShouldCallCustomerQueryService_WithCorrectQuery()
    {
        // Arrange
        var query = new GetCustomersQuery(
            SearchTerm: "customer search",
            Email: "test@example.com",
            Page: 1,
            PageSize: 20
        );

        var expectedResult = new PagedResult<CustomerDto>
        {
            Items = [],
            Page = 1,
            PageSize = 20,
            TotalCount = 0
        };

        _customerQueryServiceMock
            .Setup(x => x.GetCustomersAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _customerQueryServiceMock.Verify(
            x => x.GetCustomersAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenSuccessful()
    {
        // Arrange
        var query = new GetCustomersQuery(SearchTerm: "test", Page: 1, PageSize: 10);

        var expectedResult = new PagedResult<CustomerDto>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        _customerQueryServiceMock
            .Setup(x => x.GetCustomersAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling GetCustomersQuery")),
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
        var query = new GetCustomersQuery(SearchTerm: "test");
        var expectedException = new InvalidOperationException("Service error");

        _customerQueryServiceMock
            .Setup(x => x.GetCustomersAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Should().Be(expectedException);
    }
}