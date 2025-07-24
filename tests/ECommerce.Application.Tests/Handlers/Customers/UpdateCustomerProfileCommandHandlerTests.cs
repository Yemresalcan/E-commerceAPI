using ECommerce.Application.Commands.Customers;
using ECommerce.Application.Handlers.Customers;
using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Tests.Handlers.Customers;

public class UpdateCustomerProfileCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateCustomerProfileCommandHandler>> _loggerMock;
    private readonly UpdateCustomerProfileCommandHandler _handler;

    public UpdateCustomerProfileCommandHandlerTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UpdateCustomerProfileCommandHandler>>();
        _handler = new UpdateCustomerProfileCommandHandler(
            _customerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateCustomerProfile()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            "John",
            "Doe",
            new Email("john.doe@example.com")
        );

        var command = new UpdateCustomerProfileCommand(
            CustomerId: customerId,
            PreferredLanguage: "es",
            PreferredCurrency: PreferredCurrency.EUR,
            Timezone: "Europe/Madrid",
            CommunicationPreference: CommunicationPreference.SMS,
            ReceiveMarketingEmails: false,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: true,
            DateOfBirth: new DateTime(1990, 1, 1),
            Gender: "Male",
            Interests: "Technology, Sports"
        );

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(x => x.Update(It.IsAny<Customer>()));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _customerRepositoryMock.Verify(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _customerRepositoryMock.Verify(x => x.Update(It.IsAny<Customer>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify profile was updated
        customer.Profile.PreferredLanguage.Should().Be("es");
        customer.Profile.PreferredCurrency.Should().Be(PreferredCurrency.EUR);
        customer.Profile.Timezone.Should().Be("Europe/Madrid");
        customer.Profile.CommunicationPreference.Should().Be(CommunicationPreference.SMS);
        customer.Profile.ReceiveMarketingEmails.Should().BeFalse();
        customer.Profile.ReceiveOrderNotifications.Should().BeTrue();
        customer.Profile.ReceivePromotionalSms.Should().BeTrue();
        customer.Profile.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));
        customer.Profile.Gender.Should().Be("Male");
        customer.Profile.Interests.Should().Be("Technology, Sports");
    }

    [Fact]
    public async Task Handle_ValidCommandWithoutPersonalInfo_ShouldUpdateOnlyPreferences()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            "Jane",
            "Smith",
            new Email("jane.smith@example.com")
        );

        var command = new UpdateCustomerProfileCommand(
            CustomerId: customerId,
            PreferredLanguage: "fr",
            PreferredCurrency: PreferredCurrency.CAD,
            Timezone: "America/Toronto",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: false,
            ReceivePromotionalSms: false
        );

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(x => x.Update(It.IsAny<Customer>()));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        customer.Profile.PreferredLanguage.Should().Be("fr");
        customer.Profile.PreferredCurrency.Should().Be(PreferredCurrency.CAD);
        customer.Profile.Timezone.Should().Be("America/Toronto");
        customer.Profile.CommunicationPreference.Should().Be(CommunicationPreference.Email);
        customer.Profile.ReceiveMarketingEmails.Should().BeTrue();
        customer.Profile.ReceiveOrderNotifications.Should().BeFalse();
        customer.Profile.ReceivePromotionalSms.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new UpdateCustomerProfileCommand(
            CustomerId: customerId,
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false
        );

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Customer with ID '{customerId}' not found");
        _customerRepositoryMock.Verify(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _customerRepositoryMock.Verify(x => x.Update(It.IsAny<Customer>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidDateOfBirth_ShouldThrowArgumentException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            "John",
            "Doe",
            new Email("john.doe@example.com")
        );

        var command = new UpdateCustomerProfileCommand(
            CustomerId: customerId,
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false,
            DateOfBirth: DateTime.Today.AddDays(1) // Future date
        );

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        _customerRepositoryMock.Verify(x => x.Update(It.IsAny<Customer>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}