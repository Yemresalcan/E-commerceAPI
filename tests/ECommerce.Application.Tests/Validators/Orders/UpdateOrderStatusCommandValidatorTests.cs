using ECommerce.Application.Commands.Orders;
using ECommerce.Application.Validators.Orders;
using ECommerce.Domain.Aggregates.OrderAggregate;

namespace ECommerce.Application.Tests.Validators.Orders;

public class UpdateOrderStatusCommandValidatorTests
{
    private readonly UpdateOrderStatusCommandValidator _validator;

    public UpdateOrderStatusCommandValidatorTests()
    {
        _validator = new UpdateOrderStatusCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(
            Guid.NewGuid(),
            OrderStatus.Confirmed,
            "Order confirmed by admin"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidCommandWithoutReason_ShouldPass()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(
            Guid.NewGuid(),
            OrderStatus.Shipped
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
        var command = new UpdateOrderStatusCommand(
            Guid.Empty,
            OrderStatus.Confirmed
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.OrderId));
    }

    [Fact]
    public void Validate_InvalidOrderStatus_ShouldFail()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(
            Guid.NewGuid(),
            (OrderStatus)999 // Invalid enum value
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.NewStatus));
    }

    [Fact]
    public void Validate_TooLongReason_ShouldFail()
    {
        // Arrange
        var longReason = new string('A', 501); // 501 characters

        var command = new UpdateOrderStatusCommand(
            Guid.NewGuid(),
            OrderStatus.Cancelled,
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

        var command = new UpdateOrderStatusCommand(
            Guid.NewGuid(),
            OrderStatus.Cancelled,
            validReason
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Validate_AllValidOrderStatuses_ShouldPass(OrderStatus status)
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(
            Guid.NewGuid(),
            status
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyStringReason_ShouldPass()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(
            Guid.NewGuid(),
            OrderStatus.Confirmed,
            ""
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NullReason_ShouldPass()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(
            Guid.NewGuid(),
            OrderStatus.Confirmed,
            null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}