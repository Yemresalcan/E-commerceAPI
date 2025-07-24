using ECommerce.Application.Commands.Customers;
using ECommerce.Application.Handlers.Customers;
using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Tests.Handlers.Customers;

public class AddCustomerAddressCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AddCustomerAddressCommandHandler>> _loggerMock;
    private readonly AddCustomerAddressCommandHandler _handler;

    public AddCustomerAddressCommandHandlerTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AddCustomerAddressCommandHandler>>();
        _handler = new AddCustomerAddressCommandHandler(
            _customerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddAddressAndReturnId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            "John",
            "Doe",
            new Email("john.doe@example.com")
        );

        var command = new AddCustomerAddressCommand(
            CustomerId: customerId,
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: "USA",
            Street2: "Apt 4B",
            Label: "Home",
            IsPrimary: true
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
        result.Should().NotBeEmpty();
        _customerRepositoryMock.Verify(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _customerRepositoryMock.Verify(x => x.Update(It.IsAny<Customer>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Verify address was added
        customer.Addresses.Should().HaveCount(1);
        var address = customer.Addresses.First();
        address.Type.Should().Be(AddressType.Shipping);
        address.Street1.Should().Be("123 Main St");
        address.Street2.Should().Be("Apt 4B");
        address.City.Should().Be("New York");
        address.State.Should().Be("NY");
        address.PostalCode.Should().Be("10001");
        address.Country.Should().Be("USA");
        address.Label.Should().Be("Home");
        address.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ShouldAddAddress()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            "Jane",
            "Smith",
            new Email("jane.smith@example.com")
        );

        var command = new AddCustomerAddressCommand(
            CustomerId: customerId,
            Type: AddressType.Billing,
            Street1: "456 Oak Ave",
            City: "Los Angeles",
            State: "CA",
            PostalCode: "90210",
            Country: "USA"
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
        result.Should().NotBeEmpty();
        customer.Addresses.Should().HaveCount(1);
        var address = customer.Addresses.First();
        address.Type.Should().Be(AddressType.Billing);
        address.Street1.Should().Be("456 Oak Ave");
        address.Street2.Should().BeNull();
        address.City.Should().Be("Los Angeles");
        address.State.Should().Be("CA");
        address.PostalCode.Should().Be("90210");
        address.Country.Should().Be("USA");
        address.Label.Should().BeNull();
        address.IsPrimary.Should().BeTrue(); // First address becomes primary automatically
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new AddCustomerAddressCommand(
            CustomerId: customerId,
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: "USA"
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

    [Theory]
    [InlineData("", "New York", "NY", "10001", "USA")]
    [InlineData("123 Main St", "", "NY", "10001", "USA")]
    [InlineData("123 Main St", "New York", "", "10001", "USA")]
    [InlineData("123 Main St", "New York", "NY", "", "USA")]
    [InlineData("123 Main St", "New York", "NY", "10001", "")]
    public async Task Handle_InvalidAddressData_ShouldThrowArgumentException(
        string street1, string city, string state, string postalCode, string country)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            "John",
            "Doe",
            new Email("john.doe@example.com")
        );

        var command = new AddCustomerAddressCommand(
            CustomerId: customerId,
            Type: AddressType.Shipping,
            Street1: street1,
            City: city,
            State: state,
            PostalCode: postalCode,
            Country: country
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

    [Fact]
    public async Task Handle_SecondAddressAsPrimary_ShouldUpdatePrimaryAddress()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(
            "John",
            "Doe",
            new Email("john.doe@example.com")
        );

        // Add first address
        var firstAddress = Address.Create(
            AddressType.Shipping,
            "123 First St",
            "City1",
            "State1",
            "12345",
            "USA",
            isPrimary: true
        );
        customer.AddAddress(firstAddress);

        var command = new AddCustomerAddressCommand(
            CustomerId: customerId,
            Type: AddressType.Billing,
            Street1: "456 Second St",
            City: "City2",
            State: "State2",
            PostalCode: "67890",
            Country: "USA",
            IsPrimary: true
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
        result.Should().NotBeEmpty();
        customer.Addresses.Should().HaveCount(2);
        
        // First address should no longer be primary
        customer.Addresses.First(a => a.Street1 == "123 First St").IsPrimary.Should().BeFalse();
        
        // Second address should be primary
        customer.Addresses.First(a => a.Street1 == "456 Second St").IsPrimary.Should().BeTrue();
    }
}