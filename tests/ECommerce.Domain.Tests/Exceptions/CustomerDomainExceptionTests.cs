using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Tests.Exceptions;

public class CustomerDomainExceptionTests
{
    [Fact]
    public void DuplicateEmailException_SetsPropertiesCorrectly()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var exception = new DuplicateEmailException(email);

        // Assert
        Assert.Equal(email, exception.Email);
        Assert.Equal($"A customer with email '{email}' already exists", exception.Message);
    }

    [Fact]
    public void InvalidEmailFormatException_SetsPropertiesCorrectly()
    {
        // Arrange
        var email = "invalid-email";

        // Act
        var exception = new InvalidEmailFormatException(email);

        // Assert
        Assert.Equal(email, exception.Email);
        Assert.Equal($"Invalid email format: '{email}'", exception.Message);
    }

    [Fact]
    public void InvalidPhoneNumberFormatException_SetsPropertiesCorrectly()
    {
        // Arrange
        var phoneNumber = "invalid-phone";

        // Act
        var exception = new InvalidPhoneNumberFormatException(phoneNumber);

        // Assert
        Assert.Equal(phoneNumber, exception.PhoneNumber);
        Assert.Equal($"Invalid phone number format: '{phoneNumber}'", exception.Message);
    }

    [Fact]
    public void InvalidAddressException_SetsMessageCorrectly()
    {
        // Arrange
        var message = "Address is invalid";

        // Act
        var exception = new InvalidAddressException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void CustomerNotFoundException_SetsPropertiesCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var exception = new CustomerNotFoundException(customerId);

        // Assert
        Assert.Equal(customerId, exception.CustomerId);
        Assert.Equal($"Customer with ID '{customerId}' was not found", exception.Message);
    }

    [Fact]
    public void CustomerDomainException_WithMessage_SetsMessageCorrectly()
    {
        // Arrange
        var message = "Customer domain rule violated";

        // Act
        var exception = new CustomerDomainException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void CustomerDomainException_WithMessageAndInnerException_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Customer domain rule violated";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new CustomerDomainException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }
}