using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers.Customers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Customers;

namespace ECommerce.Application.Tests.Handlers.Customers;

public class GetCustomerQueryHandlerTests
{
    private readonly Mock<ICustomerQueryService> _customerQueryServiceMock;
    private readonly Mock<ILogger<GetCustomerQueryHandler>> _loggerMock;
    private readonly GetCustomerQueryHandler _handler;

    public GetCustomerQueryHandlerTests()
    {
        _customerQueryServiceMock = new Mock<ICustomerQueryService>();
        _loggerMock = new Mock<ILogger<GetCustomerQueryHandler>>();
        _handler = new GetCustomerQueryHandler(
            _customerQueryServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomerDto_WhenCustomerExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerQuery(customerId);

        var customerDto = new CustomerDto(
            customerId,
            "John",
            "Doe",
            "John Doe",
            "john.doe@example.com",
            "+1234567890",
            true,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(-1),
            [],
            new ProfileDto(null, null, "en", "USD", true, false, []),
            new CustomerStatisticsDto(5, 500m, "USD", 100m, DateTime.UtcNow.AddDays(-20), DateTime.UtcNow.AddDays(-1), 500m, "Regular"),
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(-1)
        );

        _customerQueryServiceMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerQuery(customerId);

        _customerQueryServiceMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallCustomerQueryService_WithCorrectId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerQuery(customerId);

        _customerQueryServiceMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _customerQueryServiceMock.Verify(
            x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenCustomerFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerQuery(customerId);

        var customerDto = new CustomerDto(
            customerId, "John", "Doe", "John Doe", "john@test.com", null, true, DateTime.UtcNow, null, [],
            new ProfileDto(null, null, "en", "USD", false, false, []),
            new CustomerStatisticsDto(0, 0, "USD", 0, null, null, 0, "New"),
            DateTime.UtcNow, DateTime.UtcNow
        );

        _customerQueryServiceMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerDto);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling GetCustomerQuery")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved customer")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenCustomerNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerQuery(customerId);

        _customerQueryServiceMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

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
        var customerId = Guid.NewGuid();
        var query = new GetCustomerQuery(customerId);
        var expectedException = new InvalidOperationException("Service error");

        _customerQueryServiceMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Should().Be(expectedException);
    }
}