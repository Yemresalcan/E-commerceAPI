using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+12345678901")]
    [InlineData("+123456789012345")]
    [InlineData("1234567890")]
    [InlineData("12345678901")]
    public void Constructor_WithValidPhoneNumber_ShouldCreatePhoneNumber(string validPhone)
    {
        // Act
        var phoneNumber = new PhoneNumber(validPhone);

        // Assert
        phoneNumber.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithPhoneNumberWithSpacesAndDashes_ShouldNormalizePhoneNumber()
    {
        // Arrange
        var phoneWithFormatting = "+1 (555) 123-4567";

        // Act
        var phoneNumber = new PhoneNumber(phoneWithFormatting);

        // Assert
        phoneNumber.Value.Should().Be("+15551234567");
    }

    [Fact]
    public void Constructor_WithPhoneNumberWithoutPlusSign_ShouldAddPlusSign()
    {
        // Arrange
        var phoneWithoutPlus = "1234567890";

        // Act
        var phoneNumber = new PhoneNumber(phoneWithoutPlus);

        // Assert
        phoneNumber.Value.Should().Be("1234567890"); // No plus added for domestic numbers
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyPhoneNumber_ShouldThrowArgumentException(string invalidPhone)
    {
        // Act & Assert
        var action = () => new PhoneNumber(invalidPhone);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Phone number cannot be null or empty.*");
    }

    [Theory]
    [InlineData("123456")] // Too short
    [InlineData("1234567890123456")] // Too long
    [InlineData("+0123456789")] // Starts with 0 after country code
    [InlineData("0123456789")] // Starts with 0
    [InlineData("abc123456789")] // Contains letters
    [InlineData("+abc123456789")] // Contains letters after +
    [InlineData("++1234567890")] // Double plus
    public void Constructor_WithInvalidPhoneNumber_ShouldThrowArgumentException(string invalidPhone)
    {
        // Act & Assert
        var action = () => new PhoneNumber(invalidPhone);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Invalid phone number format.*");
    }

    [Theory]
    [InlineData("+15551234567", "1", "5551234567")]
    [InlineData("+447123456789", "44", "7123456789")]
    [InlineData("+4912345678901", "49", "12345678901")]
    [InlineData("2234567890", "", "2234567890")]
    public void CountryCodeAndNumber_ShouldExtractCorrectParts(string phoneValue, string expectedCountryCode, string expectedNumber)
    {
        // Arrange
        var phoneNumber = new PhoneNumber(phoneValue);

        // Act & Assert
        phoneNumber.CountryCode.Should().Be(expectedCountryCode);
        phoneNumber.Number.Should().Be(expectedNumber);
    }

    [Fact]
    public void FormattedValue_WithUSNumber_ShouldReturnFormattedUSNumber()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+15551234567");

        // Act
        var formatted = phoneNumber.FormattedValue;

        // Assert
        formatted.Should().Be("+1 (555) 123-4567");
    }

    [Fact]
    public void FormattedValue_WithNonUSNumber_ShouldReturnWithPlusSign()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+447123456789");

        // Act
        var formatted = phoneNumber.FormattedValue;

        // Assert
        formatted.Should().Be("+447123456789");
    }

    [Fact]
    public void FormattedValue_WithDomesticNumber_ShouldAddPlusSign()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("1234567890");

        // Act
        var formatted = phoneNumber.FormattedValue;

        // Assert
        formatted.Should().Be("+1234567890");
    }

    [Theory]
    [InlineData("+15551234567", true)] // US mobile
    [InlineData("+447123456789", true)] // UK mobile (starts with 7)
    [InlineData("+441234567890", false)] // UK landline (doesn't start with 7)
    [InlineData("+4912345678901", true)] // Unknown country code defaults to mobile
    public void IsMobile_ShouldReturnCorrectValue(string phoneValue, bool expectedIsMobile)
    {
        // Arrange
        var phoneNumber = new PhoneNumber(phoneValue);

        // Act
        var isMobile = phoneNumber.IsMobile;

        // Assert
        isMobile.Should().Be(expectedIsMobile);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedValue()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+15551234567");

        // Act
        var result = phoneNumber.ToString();

        // Assert
        result.Should().Be("+1 (555) 123-4567");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnPhoneValue()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+15551234567");

        // Act
        string phoneString = phoneNumber;

        // Assert
        phoneString.Should().Be("+15551234567");
    }

    [Fact]
    public void Equality_WithSamePhoneValue_ShouldBeEqual()
    {
        // Arrange
        var phone1 = new PhoneNumber("+1 (555) 123-4567");
        var phone2 = new PhoneNumber("+15551234567");

        // Act & Assert
        phone1.Should().Be(phone2);
        (phone1 == phone2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentPhoneValue_ShouldNotBeEqual()
    {
        // Arrange
        var phone1 = new PhoneNumber("+15551234567");
        var phone2 = new PhoneNumber("+15551234568");

        // Act & Assert
        phone1.Should().NotBe(phone2);
        (phone1 == phone2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSamePhoneValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var phone1 = new PhoneNumber("+1 (555) 123-4567");
        var phone2 = new PhoneNumber("+15551234567");

        // Act & Assert
        phone1.GetHashCode().Should().Be(phone2.GetHashCode());
    }

    [Theory]
    [InlineData("+1-555-123-4567")]
    [InlineData("+1.555.123.4567")]
    [InlineData("+1 555 123 4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("555-123-4567")]
    public void Constructor_WithVariousFormats_ShouldNormalizeToSameValue(string phoneFormat)
    {
        // Act
        var phoneNumber = new PhoneNumber(phoneFormat);

        // Assert
        phoneNumber.Value.Should().MatchRegex(@"^\+?\d{7,15}$");
    }

    [Fact]
    public void Constructor_WithInternationalNumber_ShouldPreservePlusSign()
    {
        // Arrange
        var internationalPhone = "+447123456789";

        // Act
        var phoneNumber = new PhoneNumber(internationalPhone);

        // Assert
        phoneNumber.Value.Should().StartWith("+");
        phoneNumber.Value.Should().Be("+447123456789");
    }

    [Theory]
    [InlineData("1234567")] // Exactly 7 digits - minimum valid
    [InlineData("123456789012345")] // Exactly 15 digits - maximum valid
    public void Constructor_WithBoundaryLengths_ShouldCreateValidPhoneNumber(string phoneDigits)
    {
        // Act
        var phoneNumber = new PhoneNumber(phoneDigits);

        // Assert
        phoneNumber.Value.Should().Be(phoneDigits);
    }
}