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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyPhoneNumber_ShouldThrowArgumentException(string invalidPhone)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(invalidPhone));
        exception.Message.Should().Contain("Phone number cannot be null or empty");
    }

    [Theory]
    [InlineData("123456")] // Too short
    [InlineData("1234567890123456")] // Too long
    [InlineData("+0123456789")] // Starts with 0 after country code
    [InlineData("abc123456789")] // Contains letters
    [InlineData("++1234567890")] // Multiple plus signs
    [InlineData("123456")] // Too short after normalization
    public void Constructor_WithInvalidPhoneNumber_ShouldThrowArgumentException(string invalidPhone)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(invalidPhone));
        exception.Message.Should().Contain("Invalid phone number format");
    }

    [Fact]
    public void Constructor_WithFormattedUSNumber_ShouldNormalizeCorrectly()
    {
        // Arrange
        var formattedPhone = "+1 (555) 123-4567";

        // Act
        var phoneNumber = new PhoneNumber(formattedPhone);

        // Assert
        phoneNumber.Value.Should().Be("+15551234567");
        phoneNumber.CountryCode.Should().Be("1");
        phoneNumber.Number.Should().Be("5551234567");
    }

    [Fact]
    public void Constructor_WithInternationalNumber_ShouldParseCorrectly()
    {
        // Arrange
        var internationalPhone = "+44 20 7946 0958";

        // Act
        var phoneNumber = new PhoneNumber(internationalPhone);

        // Assert
        phoneNumber.Value.Should().Be("+442079460958");
        phoneNumber.CountryCode.Should().Be("44");
        phoneNumber.Number.Should().Be("2079460958");
    }

    [Fact]
    public void Constructor_WithGermanNumber_ShouldParseCorrectly()
    {
        // Arrange
        var germanPhone = "+49 30 12345678";

        // Act
        var phoneNumber = new PhoneNumber(germanPhone);

        // Assert
        phoneNumber.Value.Should().Be("+493012345678");
        phoneNumber.CountryCode.Should().Be("49");
        phoneNumber.Number.Should().Be("3012345678");
    }

    [Fact]
    public void Constructor_WithDomesticNumber_ShouldHandleCorrectly()
    {
        // Arrange
        var domesticPhone = "5551234567";

        // Act
        var phoneNumber = new PhoneNumber(domesticPhone);

        // Assert
        phoneNumber.Value.Should().Be("5551234567");
        phoneNumber.CountryCode.Should().Be("");
        phoneNumber.Number.Should().Be("5551234567");
    }

    [Fact]
    public void FormattedValue_WithUSNumber_ShouldFormatCorrectly()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+15551234567");

        // Act
        var formatted = phoneNumber.FormattedValue;

        // Assert
        formatted.Should().Be("+1 (555) 123-4567");
    }

    [Fact]
    public void FormattedValue_WithInternationalNumber_ShouldReturnWithPlus()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+442079460958");

        // Act
        var formatted = phoneNumber.FormattedValue;

        // Assert
        formatted.Should().Be("+442079460958");
    }

    [Fact]
    public void FormattedValue_WithDomesticNumber_ShouldAddPlus()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("5551234567");

        // Act
        var formatted = phoneNumber.FormattedValue;

        // Assert
        formatted.Should().Be("+5551234567");
    }

    [Theory]
    [InlineData("+15551234567", true)] // US mobile
    [InlineData("+447123456789", true)] // UK mobile (starts with 7)
    [InlineData("+441234567890", false)] // UK landline (doesn't start with 7)
    [InlineData("+493012345678", true)] // Unknown country code defaults to mobile
    public void IsMobile_ShouldReturnCorrectResult(string phone, bool expectedIsMobile)
    {
        // Arrange
        var phoneNumber = new PhoneNumber(phone);

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
    public void ImplicitConversion_ShouldReturnPhoneValue()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+15551234567");

        // Act
        string phoneString = phoneNumber;

        // Assert
        phoneString.Should().Be("+15551234567");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var phone1 = new PhoneNumber("+15551234567");
        var phone2 = new PhoneNumber("+1 (555) 123-4567");

        // Act & Assert
        phone1.Should().Be(phone2);
        (phone1 == phone2).Should().BeTrue();
        phone1.GetHashCode().Should().Be(phone2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var phone1 = new PhoneNumber("+15551234567");
        var phone2 = new PhoneNumber("+15551234568");

        // Act & Assert
        phone1.Should().NotBe(phone2);
        (phone1 == phone2).Should().BeFalse();
    }

    [Theory]
    [InlineData("123-456-7890")]
    [InlineData("(555) 123-4567")]
    [InlineData("555.123.4567")]
    [InlineData("555 123 4567")]
    public void Constructor_WithVariousFormats_ShouldNormalizeCorrectly(string formattedPhone)
    {
        // Act
        var phoneNumber = new PhoneNumber(formattedPhone);

        // Assert
        phoneNumber.Value.Should().NotBeNullOrEmpty();
        phoneNumber.Value.Should().MatchRegex(@"^\+?\d+$");
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("123abc456")]
    [InlineData("!@#$%")]
    public void Constructor_WithInvalidCharacters_ShouldThrowArgumentException(string invalidPhone)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(invalidPhone));
        exception.Message.Should().Contain("Invalid phone number format");
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new PhoneNumber(""));
        exception.Message.Should().Contain("Phone number cannot be null or empty");
    }

    [Fact]
    public void Constructor_WithMinimumValidLength_ShouldCreatePhoneNumber()
    {
        // Arrange
        var minPhone = "1234567"; // 7 digits minimum

        // Act
        var phoneNumber = new PhoneNumber(minPhone);

        // Assert
        phoneNumber.Value.Should().Be("1234567");
    }

    [Fact]
    public void Constructor_WithMaximumValidLength_ShouldCreatePhoneNumber()
    {
        // Arrange
        var maxPhone = "+123456789012345"; // 15 digits maximum

        // Act
        var phoneNumber = new PhoneNumber(maxPhone);

        // Assert
        phoneNumber.Value.Should().Be("+123456789012345");
    }

    [Fact]
    public void Constructor_WithWhitespace_ShouldTrimAndNormalize()
    {
        // Arrange
        var phoneWithWhitespace = "  +1 555 123 4567  ";

        // Act
        var phoneNumber = new PhoneNumber(phoneWithWhitespace);

        // Assert
        phoneNumber.Value.Should().Be("+15551234567");
    }
}