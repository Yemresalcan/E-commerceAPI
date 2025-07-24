using ECommerce.Application.Commands.Orders;
using ECommerce.Application.Validators.Orders;

namespace ECommerce.Application.Tests.Validators.Orders;

public class CancelOrderCommandValidatorTests
{
    private readonly CancelOrderCommandValidator _validator;

    public CancelOrderCommandValidatorTests()
    {
        _validator = new CancelOrderCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.NewGuid(),
            "Customer requested cancellation"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyOrderId_ShouldFail()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.Empty,
            "Valid reason"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.OrderId));
    }

    [Fact]
    public void Validate_EmptyReason_ShouldFail()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.NewGuid(),
            ""
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Reason));
    }

    [Fact]
    public void Validate_NullReason_ShouldFail()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.NewGuid(),
            null!
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Reason));
    }

    [Fact]
    public void Validate_WhitespaceReason_ShouldFail()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.NewGuid(),
            "   "
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Reason));
    }

    [Fact]
    public void Validate_TooLongReason_ShouldFail()
    {
        // Arrange
        var longReason = new string('A', 501); // 501 characters

        var command = new CancelOrderCommand(
            Guid.NewGuid(),
            longReason
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Reason));
    }

    [Fact]
    public void Validate_ValidReasonLength_ShouldPass()
    {
        // Arrange
        var validReason = new string('A', 500); // Exactly 500 characters

        var command = new CancelOrderCommand(
            Guid.NewGuid(),
            validReason
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidReasonWithSpecialCharacters_ShouldPass()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.NewGuid(),
            "Customer changed mind - wants different color & size (urgent!)"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MinimumValidReason_ShouldPass()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.NewGuid(),
            "X" // Single character
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}