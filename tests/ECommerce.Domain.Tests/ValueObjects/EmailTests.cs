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
    [InlineData("test.email.with+symbol@example.com")]
    public void Constructor_WithValidEmail_ShouldCreateEmail(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Fact]
    public void Constructor_WithValidEmailWithWhitespace_ShouldTrimAndCreateEmail()
    {
        // Arrange
        var emailWithWhitespace = "  test@example.com  ";

        // Act
        var email = new Email(emailWithWhitespace);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_WithUppercaseEmail_ShouldConvertToLowercase()
    {
        // Arrange
        var uppercaseEmail = "TEST@EXAMPLE.COM";

        // Act
        var email = new Email(uppercaseEmail);

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var action = () => new Email(invalidEmail);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be null or empty.*");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test@@example.com")]
    [InlineData("test@example")]
    [InlineData("test.example.com")]
    [InlineData("test@.example.com")]
    [InlineData("test@example.")]
    [InlineData("test@example..com")]
    [InlineData(".test@example.com")]
    [InlineData("test.@example.com")]
    [InlineData("te..st@example.com")]
    [InlineData("test@-example.com")]
    [InlineData("test@example-.com")]
    public void Constructor_WithInvalidEmailFormat_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var action = () => new Email(invalidEmail);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Invalid email format.*");
    }

    [Fact]
    public void Constructor_WithTooLongEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var longLocalPart = new string('a', 250);
        var longEmail = $"{longLocalPart}@example.com";

        // Act & Assert
        var action = () => new Email(longEmail);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email address is too long. Maximum length is 254 characters.*");
    }

    [Fact]
    public void Constructor_WithTooLongLocalPart_ShouldThrowArgumentException()
    {
        // Arrange
        var longLocalPart = new string('a', 65); // More than 64 characters
        var longEmail = $"{longLocalPart}@example.com";

        // Act & Assert
        var action = () => new Email(longEmail);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Invalid email format.*");
    }

    [Fact]
    public void Constructor_WithTooLongDomain_ShouldThrowArgumentException()
    {
        // Arrange
        var longDomain = new string('a', 250) + ".com";
        var longEmail = $"test@{longDomain}";

        // Act & Assert
        var action = () => new Email(longEmail);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Email address is too long. Maximum length is 254 characters.*");
    }

    [Fact]
    public void LocalPart_ShouldReturnCorrectLocalPart()
    {
        // Arrange
        var email = new Email("test.user@example.com");

        // Act
        var localPart = email.LocalPart;

        // Assert
        localPart.Should().Be("test.user");
    }

    [Fact]
    public void Domain_ShouldReturnCorrectDomain()
    {
        // Arrange
        var email = new Email("test@example.co.uk");

        // Act
        var domain = email.Domain;

        // Assert
        domain.Should().Be("example.co.uk");
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
    public void ImplicitConversion_ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        string emailString = email;

        // Assert
        emailString.Should().Be("test@example.com");
    }

    [Fact]
    public void Equality_WithSameEmailValue_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM");

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentEmailValue_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 == email2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameEmailValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM");

        // Act & Assert
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Theory]
    [InlineData("test@example.com", "test", "example.com")]
    [InlineData("user.name@sub.domain.co.uk", "user.name", "sub.domain.co.uk")]
    [InlineData("admin@localhost.localdomain", "admin", "localhost.localdomain")]
    public void LocalPartAndDomain_ShouldExtractCorrectParts(string emailValue, string expectedLocal, string expectedDomain)
    {
        // Arrange
        var email = new Email(emailValue);

        // Act & Assert
        email.LocalPart.Should().Be(expectedLocal);
        email.Domain.Should().Be(expectedDomain);
    }
}