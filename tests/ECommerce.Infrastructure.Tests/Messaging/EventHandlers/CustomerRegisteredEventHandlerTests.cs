using ECommerce.Domain.Events;
using ECommerce.Infrastructure.Messaging.EventHandlers;
using ECommerce.ReadModel.Models;
using ECommerce.ReadModel.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Infrastructure.Tests.Messaging.EventHandlers;

/// <summary>
/// Integration tests for CustomerRegisteredEventHandler
/// </summary>
public class CustomerRegisteredEventHandlerTests
{
    private readonly Mock<ILogger<CustomerRegisteredEventHandler>> _loggerMock;
    private readonly Mock<ICustomerSearchService> _customerSearchServiceMock;
    private readonly CustomerRegisteredEventHandler _handler;

    public CustomerRegisteredEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CustomerRegisteredEventHandler>>();
        _customerSearchServiceMock = new Mock<ICustomerSearchService>();
        _handler = new CustomerRegisteredEventHandler(_loggerMock.Object, _customerSearchServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_ShouldIndexCustomerInElasticsearch()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var email = "john.doe@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var phoneNumber = "+1234567890";
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            email,
            firstName,
            lastName,
            phoneNumber
        );

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<CustomerReadModel>(c => 
                    c.Id == customerId &&
                    c.FirstName == firstName &&
                    c.LastName == lastName &&
                    c.FullName == "John Doe" &&
                    c.Email == email &&
                    c.PhoneNumber == phoneNumber &&
                    c.IsActive == true &&
                    c.RegistrationDate == domainEvent.RegistrationDate &&
                    c.LastActiveDate == domainEvent.RegistrationDate
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_CustomerWithoutPhoneNumber_ShouldIndexSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var email = "jane.smith@example.com";
        var firstName = "Jane";
        var lastName = "Smith";
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            email,
            firstName,
            lastName,
            null // No phone number
        );

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<CustomerReadModel>(c => 
                    c.PhoneNumber == null &&
                    c.FullName == "Jane Smith"
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_ShouldSetDefaultProfileValues()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            "test@example.com",
            "Test",
            "User"
        );

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<CustomerReadModel>(c => 
                    c.Profile.PreferredLanguage == "en" &&
                    c.Profile.PreferredCurrency == "USD" &&
                    c.Profile.MarketingEmailsEnabled == true &&
                    c.Profile.SmsNotificationsEnabled == false &&
                    c.Profile.Interests.Count == 0
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_ShouldSetDefaultStatistics()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            "stats@example.com",
            "Stats",
            "Customer"
        );

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<CustomerReadModel>(c => 
                    c.Statistics.TotalOrders == 0 &&
                    c.Statistics.TotalSpent == 0 &&
                    c.Statistics.Currency == "USD" &&
                    c.Statistics.AverageOrderValue == 0 &&
                    c.Statistics.LifetimeValue == 0 &&
                    c.Statistics.Segment == "New"
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
        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            "timestamp@example.com",
            "Time",
            "Stamp"
        );

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<CustomerReadModel>(c => 
                    c.CreatedAt == domainEvent.OccurredOn &&
                    c.UpdatedAt == domainEvent.OccurredOn
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_ShouldSetCorrectSuggestField()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var email = "suggest@example.com";
        var firstName = "Suggest";
        var lastName = "Test";
        var fullName = "Suggest Test";
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            email,
            firstName,
            lastName
        );

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<CustomerReadModel>(c => 
                    c.Suggest.Input.Contains(email) &&
                    c.Suggest.Input.Contains(fullName)
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
        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            "fail@example.com",
            "Fail",
            "Test"
        );

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to index customer {customerId}")),
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
        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            "exception@example.com",
            "Exception",
            "Test"
        );

        var exception = new InvalidOperationException("Elasticsearch connection failed");
        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error handling CustomerRegisteredEvent for customer {customerId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_EmptyNames_ShouldHandleGracefully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            "empty@example.com",
            "",
            ""
        );

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<CustomerReadModel>(c => 
                    c.FirstName == "" &&
                    c.LastName == "" &&
                    c.FullName == "" // Trimmed empty string
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_SingleName_ShouldSetFullNameCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var domainEvent = new CustomerRegisteredEvent(
            customerId,
            "single@example.com",
            "Madonna",
            ""
        );

        _customerSearchServiceMock
            .Setup(x => x.IndexDocumentAsync(It.IsAny<CustomerReadModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _customerSearchServiceMock.Verify(
            x => x.IndexDocumentAsync(
                It.Is<CustomerReadModel>(c => 
                    c.FirstName == "Madonna" &&
                    c.LastName == "" &&
                    c.FullName == "Madonna"
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}