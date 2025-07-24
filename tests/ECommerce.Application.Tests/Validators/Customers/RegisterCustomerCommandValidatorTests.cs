using ECommerce.Application.Commands.Customers;
using ECommerce.Application.Validators.Customers;

namespace ECommerce.Application.Tests.Validators.Customers;

public class RegisterCustomerCommandValidatorTests
{
    private readonly RegisterCustomerCommandValidator _validator;

    public RegisterCustomerCommandValidatorTests()
    {
        _validator = new RegisterCustomerCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            PhoneNumber: "+1234567890"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutPhoneNumber_ShouldBeValid()
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: "Jane",
            LastName: "Smith",
            Email: "jane.smith@example.com"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Doe", "john.doe@example.com", "First name is required")]
    [InlineData("   ", "Doe", "john.doe@example.com", "First name is required")]
    [InlineData(null, "Doe", "john.doe@example.com", "First name is required")]
    public void Validate_InvalidFirstName_ShouldBeInvalid(string firstName, string lastName, string email, string expectedError)
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: firstName,
            LastName: lastName,
            Email: email
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Theory]
    [InlineData("John", "", "john.doe@example.com", "Last name is required")]
    [InlineData("John", "   ", "john.doe@example.com", "Last name is required")]
    [InlineData("John", null, "john.doe@example.com", "Last name is required")]
    public void Validate_InvalidLastName_ShouldBeInvalid(string firstName, string lastName, string email, string expectedError)
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: firstName,
            LastName: lastName,
            Email: email
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Theory]
    [InlineData("John", "Doe", "", "Email is required")]
    [InlineData("John", "Doe", "   ", "Email is required")]
    [InlineData("John", "Doe", null, "Email is required")]
    [InlineData("John", "Doe", "invalid-email", "Email must be a valid email address")]
    [InlineData("John", "Doe", "invalid@", "Email must be a valid email address")]
    [InlineData("John", "Doe", "@invalid.com", "Email must be a valid email address")]
    public void Validate_InvalidEmail_ShouldBeInvalid(string firstName, string lastName, string email, string expectedError)
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: firstName,
            LastName: lastName,
            Email: email
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public void Validate_FirstNameTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: new string('A', 51), // 51 characters
            LastName: "Doe",
            Email: "john.doe@example.com"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "First name cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_LastNameTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: "John",
            LastName: new string('D', 51), // 51 characters
            Email: "john.doe@example.com"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Last name cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldBeInvalid()
    {
        // Arrange
        var longEmail = new string('a', 244) + "@example.com"; // 256 characters
        var command = new RegisterCustomerCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: longEmail
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email cannot exceed 255 characters");
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+44123456789")]
    [InlineData("+33123456789")]
    [InlineData("1234567890")]
    public void Validate_ValidPhoneNumber_ShouldBeValid(string phoneNumber)
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            PhoneNumber: phoneNumber
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc123")]
    [InlineData("+0123456789")]
    [InlineData("123456789012345678")] // Too long
    public void Validate_InvalidPhoneNumber_ShouldBeInvalid(string phoneNumber)
    {
        // Arrange
        var command = new RegisterCustomerCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            PhoneNumber: phoneNumber
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Phone number must be a valid international format");
    }
}