using ECommerce.Application.Commands.Customers;
using ECommerce.Application.Validators.Customers;
using ECommerce.Domain.Aggregates.CustomerAggregate;

namespace ECommerce.Application.Tests.Validators.Customers;

public class UpdateCustomerProfileCommandValidatorTests
{
    private readonly UpdateCustomerProfileCommandValidator _validator;

    public UpdateCustomerProfileCommandValidatorTests()
    {
        _validator = new UpdateCustomerProfileCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false,
            DateOfBirth: new DateTime(1990, 1, 1),
            Gender: "Male",
            Interests: "Technology, Sports"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutOptionalFields_ShouldBeValid()
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "es",
            PreferredCurrency: PreferredCurrency.EUR,
            Timezone: "Europe/Madrid",
            CommunicationPreference: CommunicationPreference.SMS,
            ReceiveMarketingEmails: false,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: true
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
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.Empty,
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Customer ID is required");
    }

    [Theory]
    [InlineData("", "Preferred language is required")]
    [InlineData("   ", "Preferred language is required")]
    [InlineData(null, "Preferred language is required")]
    [InlineData("e", "Preferred language must be a 2-character ISO 639-1 code")]
    [InlineData("eng", "Preferred language must be a 2-character ISO 639-1 code")]
    public void Validate_InvalidPreferredLanguage_ShouldBeInvalid(string preferredLanguage, string expectedError)
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: preferredLanguage,
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Theory]
    [InlineData("", "Timezone is required")]
    [InlineData("   ", "Timezone is required")]
    [InlineData(null, "Timezone is required")]
    public void Validate_InvalidTimezone_ShouldBeInvalid(string timezone, string expectedError)
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: timezone,
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public void Validate_TimezoneTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: new string('A', 51), // 51 characters
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Timezone cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_FutureDateOfBirth_ShouldBeInvalid()
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false,
            DateOfBirth: DateTime.Today.AddDays(1)
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Date of birth cannot be in the future");
    }

    [Fact]
    public void Validate_DateOfBirthTooOld_ShouldBeInvalid()
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false,
            DateOfBirth: DateTime.Today.AddYears(-151)
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Date of birth cannot be more than 150 years ago");
    }

    [Fact]
    public void Validate_GenderTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false,
            Gender: new string('M', 21) // 21 characters
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Gender cannot exceed 20 characters");
    }

    [Fact]
    public void Validate_InterestsTooLong_ShouldBeInvalid()
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false,
            Interests: new string('A', 501) // 501 characters
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Interests cannot exceed 500 characters");
    }

    [Theory]
    [InlineData(PreferredCurrency.USD)]
    [InlineData(PreferredCurrency.EUR)]
    [InlineData(PreferredCurrency.GBP)]
    [InlineData(PreferredCurrency.CAD)]
    [InlineData(PreferredCurrency.AUD)]
    [InlineData(PreferredCurrency.JPY)]
    public void Validate_ValidPreferredCurrency_ShouldBeValid(PreferredCurrency currency)
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "en",
            PreferredCurrency: currency,
            Timezone: "UTC",
            CommunicationPreference: CommunicationPreference.Email,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(CommunicationPreference.Email)]
    [InlineData(CommunicationPreference.SMS)]
    [InlineData(CommunicationPreference.Phone)]
    [InlineData(CommunicationPreference.None)]
    public void Validate_ValidCommunicationPreference_ShouldBeValid(CommunicationPreference preference)
    {
        // Arrange
        var command = new UpdateCustomerProfileCommand(
            CustomerId: Guid.NewGuid(),
            PreferredLanguage: "en",
            PreferredCurrency: PreferredCurrency.USD,
            Timezone: "UTC",
            CommunicationPreference: preference,
            ReceiveMarketingEmails: true,
            ReceiveOrderNotifications: true,
            ReceivePromotionalSms: false
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}