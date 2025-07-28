using ECommerce.Application.Exceptions;
using FluentValidation.Results;

namespace ECommerce.Application.Tests.Exceptions;

public class ApplicationExceptionTests
{
    [Fact]
    public void NotFoundException_WithResourceNameAndId_SetsPropertiesCorrectly()
    {
        // Arrange
        var resourceName = "Product";
        var resourceId = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(resourceName, resourceId);

        // Assert
        Assert.Equal(resourceName, exception.ResourceName);
        Assert.Equal(resourceId, exception.ResourceId);
        Assert.Equal($"{resourceName} with ID '{resourceId}' was not found", exception.Message);
    }

    [Fact]
    public void NotFoundException_WithMessage_SetsMessageCorrectly()
    {
        // Arrange
        var message = "Custom not found message";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(string.Empty, exception.ResourceName);
        Assert.Equal(string.Empty, exception.ResourceId);
    }

    [Fact]
    public void ValidationException_WithNoParameters_InitializesEmptyErrors()
    {
        // Act
        var exception = new ECommerce.Application.Exceptions.ValidationException();

        // Assert
        Assert.Equal("One or more validation failures have occurred.", exception.Message);
        Assert.Empty(exception.Errors);
    }

    [Fact]
    public void ValidationException_WithValidationFailures_GroupsErrorsByProperty()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Name", "Name must be at least 3 characters"),
            new("Email", "Email is invalid")
        };

        // Act
        var exception = new ECommerce.Application.Exceptions.ValidationException(failures);

        // Assert
        Assert.Equal(2, exception.Errors.Count);
        Assert.Contains("Name", exception.Errors.Keys);
        Assert.Contains("Email", exception.Errors.Keys);
        Assert.Equal(2, exception.Errors["Name"].Length);
        Assert.Single(exception.Errors["Email"]);
        Assert.Contains("Name is required", exception.Errors["Name"]);
        Assert.Contains("Name must be at least 3 characters", exception.Errors["Name"]);
        Assert.Contains("Email is invalid", exception.Errors["Email"]);
    }

    [Fact]
    public void ValidationException_WithPropertyAndMessage_SetsErrorCorrectly()
    {
        // Arrange
        var propertyName = "Email";
        var errorMessage = "Email is required";

        // Act
        var exception = new ECommerce.Application.Exceptions.ValidationException(propertyName, errorMessage);

        // Assert
        Assert.Single(exception.Errors);
        Assert.Contains(propertyName, exception.Errors.Keys);
        Assert.Single(exception.Errors[propertyName]);
        Assert.Equal(errorMessage, exception.Errors[propertyName][0]);
    }

    [Fact]
    public void ConflictException_WithMessage_SetsMessageCorrectly()
    {
        // Arrange
        var message = "Resource already exists";

        // Act
        var exception = new ConflictException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ConflictException_WithMessageAndInnerException_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Resource already exists";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new ConflictException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }
}