using ECommerce.Application.Commands.Orders;
using ECommerce.Application.DTOs;
using ECommerce.Application.Validators.Orders;

namespace ECommerce.Application.Tests.Validators.Orders;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator;

    public PlaceOrderCommandValidatorTests()
    {
        _validator = new PlaceOrderCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), "Test Product", 1, 50m, "USD", 0)
        };

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            "123 Main St, City, State",
            "456 Billing St, City, State",
            orderItems
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyCustomerId_ShouldFail()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), "Test Product", 1, 50m, "USD", 0)
        };

        var command = new PlaceOrderCommand(
            Guid.Empty,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CustomerId));
    }

    [Fact]
    public void Validate_EmptyShippingAddress_ShouldFail()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), "Test Product", 1, 50m, "USD", 0)
        };

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            "",
            "456 Billing St",
            orderItems
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ShippingAddress));
    }

    [Fact]
    public void Validate_EmptyBillingAddress_ShouldFail()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), "Test Product", 1, 50m, "USD", 0)
        };

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            "123 Main St",
            "",
            orderItems
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.BillingAddress));
    }

    [Fact]
    public void Validate_EmptyOrderItems_ShouldFail()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            "123 Main St",
            "456 Billing St",
            new List<OrderItemDto>()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.OrderItems));
    }

    [Fact]
    public void Validate_TooLongShippingAddress_ShouldFail()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), "Test Product", 1, 50m, "USD", 0)
        };

        var longAddress = new string('A', 501); // 501 characters

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            longAddress,
            "456 Billing St",
            orderItems
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ShippingAddress));
    }

    [Fact]
    public void Validate_InvalidOrderItem_ShouldFail()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.Empty, "", 0, -10m, "INVALID", -5m) // All invalid values
        };

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ProductId"));
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ProductName"));
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Quantity"));
        result.Errors.Should().Contain(e => e.PropertyName.Contains("UnitPrice"));
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Currency"));
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Discount"));
    }

    [Fact]
    public void Validate_ValidOrderItemWithDiscount_ShouldPass()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), "Test Product", 2, 100m, "USD", 10m)
        };

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MultipleOrderItems_ShouldValidateAll()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), "Product 1", 1, 50m, "USD", 0),
            new(Guid.NewGuid(), "Product 2", 2, 30m, "USD", 5m),
            new(Guid.Empty, "", 0, -10m, "XX", -1m) // Invalid item
        };

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        // Should have errors only for the third item
        result.Errors.Should().Contain(e => e.PropertyName.Contains("[2]"));
    }
}