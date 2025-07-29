using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.org")]
    [InlineData("user_name@example-domain.com")]
    [InlineData("123@example.com")]
    [InlineData("test@sub.domain.com")]
    public void Constructor_WithValidEmail_ShouldCreateEmail(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Email(invalidEmail));
        exception.Message.Should().Contain("Email cannot be null or empty");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test..test@example.com")]
    [InlineData("test@example")]
    [InlineData("test@.example.com")]
    [InlineData("test@example..com")]
    [InlineData("test@example.")]
    [InlineData("test@-example.com")]
    [InlineData("test@example-.com")]
    [InlineData(".test@example.com")]
    [InlineData("test.@example.com")]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Email(invalidEmail));
        exception.Message.Should().Contain("Invalid email format");
    }

    [Fact]
    public void Constructor_WithEmailTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // Over 254 characters

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Email(longEmail));
        exception.Message.Should().Contain("Email address is too long");
    }

    [Fact]
    public void Constructor_WithLocalPartTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longLocalPart = new string('a', 65); // Over 64 characters
        var email = $"{longLocalPart}@example.com";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Email(email));
        exception.Message.Should().Contain("Invalid email format");
    }

    [Fact]
    public void Constructor_WithDomainTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longDomain = new string('a', 250) + ".com"; // Over 253 characters
        var email = $"test@{longDomain}";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Email(email));
        exception.Message.Should().Contain("Email address is too long");
    }

    [Fact]
    public void Constructor_WithWhitespace_ShouldTrimAndCreateEmail()
    {
        // Arrange
        var emailWithWhitespace = "  test@example.com  ";

        // Act
        var email = new Email(emailWithWhitespace);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_WithMixedCase_ShouldConvertToLowerCase()
    {
        // Arrange
        var mixedCaseEmail = "Test.User@EXAMPLE.COM";

        // Act
        var email = new Email(mixedCaseEmail);

        // Assert
        email.Value.Should().Be("test.user@example.com");
    }

    [Fact]
    public void LocalPart_ShouldReturnCorrectValue()
    {
        // Arrange
        var email = new Email("test.user@example.com");

        // Act
        var localPart = email.LocalPart;

        // Assert
        localPart.Should().Be("test.user");
    }

    [Fact]
    public void Domain_ShouldReturnCorrectValue()
    {
        // Arrange
        var email = new Email("test.user@example.com");

        // Act
        var domain = email.Domain;

        // Assert
        domain.Should().Be("example.com");
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var emailValue = "test@example.com";
        var email = new Email(emailValue);

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be(emailValue);
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnEmailValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        string emailString = email;

        // Assert
        emailString.Should().Be("test@example.com");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM");

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 == email2).Should().BeFalse();
    }

    [Theory]
    [InlineData("test@sub.domain.example.com")]
    [InlineData("very.long.local.part@example.com")]
    [InlineData("test123@example123.com")]
    public void Constructor_WithComplexValidEmails_ShouldCreateEmail(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("test@@example.com")]
    [InlineData("test@example@com")]
    public void Constructor_WithMultipleAtSigns_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Email(invalidEmail));
        exception.Message.Should().Contain("Invalid email format");
    }
}