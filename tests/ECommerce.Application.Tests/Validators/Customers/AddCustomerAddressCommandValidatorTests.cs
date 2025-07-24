using ECommerce.Application.Commands.Customers;
using ECommerce.Application.Validators.Customers;
using ECommerce.Domain.Aggregates.CustomerAggregate;

namespace ECommerce.Application.Tests.Validators.Customers;

public class AddCustomerAddressCommandValidatorTests
{
    private readonly AddCustomerAddressCommandValidator _validator;

    public AddCustomerAddressCommandValidatorTests()
    {
        _validator = new AddCustomerAddressCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
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

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithMinimalData_ShouldBeValid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Billing,
            Street1: "456 Oak Ave",
            City: "Los Angeles",
            State: "CA",
            PostalCode: "90210",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyCustomerId_ShouldBeInvalid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.Empty,
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Customer ID is required");
    }

    [Theory]
    [InlineData("", "Street address is required")]
    [InlineData("   ", "Street address is required")]
    [InlineData(null, "Street address is required")]
    public void Validate_InvalidStreet1_ShouldBeInvalid(string street1, string expectedError)
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: street1,
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public void Validate_Street1TooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: new string('A', 101), // 101 characters
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Street address cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_Street2TooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: "USA",
            Street2: new string('B', 101) // 101 characters
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Street address line 2 cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("", "City is required")]
    [InlineData("   ", "City is required")]
    [InlineData(null, "City is required")]
    public void Validate_InvalidCity_ShouldBeInvalid(string city, string expectedError)
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: city,
            State: "NY",
            PostalCode: "10001",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public void Validate_CityTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: new string('C', 51), // 51 characters
            State: "NY",
            PostalCode: "10001",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "City cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("", "State is required")]
    [InlineData("   ", "State is required")]
    [InlineData(null, "State is required")]
    public void Validate_InvalidState_ShouldBeInvalid(string state, string expectedError)
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: state,
            PostalCode: "10001",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public void Validate_StateTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: new string('S', 51), // 51 characters
            PostalCode: "10001",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "State cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("", "Postal code is required")]
    [InlineData("   ", "Postal code is required")]
    [InlineData(null, "Postal code is required")]
    public void Validate_InvalidPostalCode_ShouldBeInvalid(string postalCode, string expectedError)
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: postalCode,
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public void Validate_PostalCodeTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: new string('1', 21), // 21 characters
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Postal code cannot exceed 20 characters");
    }

    [Theory]
    [InlineData("", "Country is required")]
    [InlineData("   ", "Country is required")]
    [InlineData(null, "Country is required")]
    public void Validate_InvalidCountry_ShouldBeInvalid(string country, string expectedError)
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: country
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public void Validate_CountryTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: new string('U', 51) // 51 characters
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Country cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_LabelTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: AddressType.Shipping,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: "USA",
            Label: new string('L', 51) // 51 characters
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Address label cannot exceed 50 characters");
    }

    [Theory]
    [InlineData(AddressType.Shipping)]
    [InlineData(AddressType.Billing)]
    [InlineData(AddressType.Both)]
    public void Validate_ValidAddressType_ShouldBeValid(AddressType addressType)
    {
        // Arrange
        var command = new AddCustomerAddressCommand(
            CustomerId: Guid.NewGuid(),
            Type: addressType,
            Street1: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}