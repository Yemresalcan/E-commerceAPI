using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;
using FluentAssertions;

namespace ECommerce.Domain.Tests.Aggregates.OrderAggregate;

public class OrderItemTests
{
    private readonly Guid _productId = Guid.NewGuid();
    private readonly string _productName = "Test Product";
    private readonly Money _unitPrice = new(100.00m, "USD");
    private readonly Money _discount = new(10.00m, "USD");

    [Fact]
    public void Create_WithValidData_ShouldCreateOrderItem()
    {
        // Act
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);

        // Assert
        orderItem.Should().NotBeNull();
        orderItem.Id.Should().NotBeEmpty();
        orderItem.ProductId.Should().Be(_productId);
        orderItem.ProductName.Should().Be(_productName);
        orderItem.Quantity.Should().Be(2);
        orderItem.UnitPrice.Should().Be(_unitPrice);
        orderItem.Discount.Should().Be(Money.Zero("USD"));
        orderItem.TotalPrice.Amount.Should().Be(200.00m);
        orderItem.TotalPrice.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithDiscount_ShouldCreateOrderItemWithDiscount()
    {
        // Act
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, _discount);

        // Assert
        orderItem.Discount.Should().Be(_discount);
        orderItem.TotalPrice.Amount.Should().Be(190.00m); // (2 * 100) - 10
    }

    [Fact]
    public void Create_WithEmptyProductId_ShouldThrowException()
    {
        // Act & Assert
        var act = () => OrderItem.Create(Guid.Empty, _productName, 1, _unitPrice);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Product ID cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyProductName_ShouldThrowException()
    {
        // Act & Assert
        var act = () => OrderItem.Create(_productId, "", 1, _unitPrice);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Product name is required");
    }

    [Fact]
    public void Create_WithNullProductName_ShouldThrowException()
    {
        // Act & Assert
        var act = () => OrderItem.Create(_productId, null!, 1, _unitPrice);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Product name is required");
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrowException()
    {
        // Act & Assert
        var act = () => OrderItem.Create(_productId, _productName, 0, _unitPrice);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Quantity must be greater than zero");
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldThrowException()
    {
        // Act & Assert
        var act = () => OrderItem.Create(_productId, _productName, -1, _unitPrice);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Quantity must be greater than zero");
    }

    [Fact]
    public void Create_WithNegativeUnitPrice_ShouldThrowException()
    {
        // Arrange
        var negativePrice = new Money(-50.00m, "USD");

        // Act & Assert
        var act = () => OrderItem.Create(_productId, _productName, 1, negativePrice);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Unit price cannot be negative");
    }

    [Fact]
    public void Create_WithDiscountDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var discountDifferentCurrency = new Money(10.00m, "EUR");

        // Act & Assert
        var act = () => OrderItem.Create(_productId, _productName, 1, _unitPrice, discountDifferentCurrency);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Discount currency must match unit price currency");
    }

    [Fact]
    public void Create_WithDiscountExceedingTotalValue_ShouldThrowException()
    {
        // Arrange
        var excessiveDiscount = new Money(250.00m, "USD"); // More than 2 * 100

        // Act & Assert
        var act = () => OrderItem.Create(_productId, _productName, 2, _unitPrice, excessiveDiscount);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Discount cannot exceed total item value");
    }

    [Fact]
    public void UpdateQuantity_WithValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);

        // Act
        orderItem.UpdateQuantity(5);

        // Assert
        orderItem.Quantity.Should().Be(5);
        orderItem.TotalPrice.Amount.Should().Be(500.00m);
    }

    [Fact]
    public void UpdateQuantity_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);

        // Act & Assert
        var act = () => orderItem.UpdateQuantity(0);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Quantity must be greater than zero");
    }

    [Fact]
    public void UpdateQuantity_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);

        // Act & Assert
        var act = () => orderItem.UpdateQuantity(-1);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Quantity must be greater than zero");
    }

    [Fact]
    public void UpdateQuantity_WhenDiscountExceedsNewTotal_ShouldThrowException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 5, _unitPrice, new Money(400.00m, "USD"));

        // Act & Assert - Reducing quantity to 2 would make total 200, but discount is 400
        var act = () => orderItem.UpdateQuantity(2);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Current discount exceeds new total item value");
    }

    [Fact]
    public void ApplyDiscount_WithValidDiscount_ShouldApplyDiscount()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var newDiscount = new Money(20.00m, "USD");

        // Act
        orderItem.ApplyDiscount(newDiscount);

        // Assert
        orderItem.Discount.Should().Be(newDiscount);
        orderItem.TotalPrice.Amount.Should().Be(180.00m); // (2 * 100) - 20
    }

    [Fact]
    public void ApplyDiscount_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var discountDifferentCurrency = new Money(20.00m, "EUR");

        // Act & Assert
        var act = () => orderItem.ApplyDiscount(discountDifferentCurrency);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Discount currency must match unit price currency");
    }

    [Fact]
    public void ApplyDiscount_WithNegativeDiscount_ShouldThrowException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var negativeDiscount = new Money(-10.00m, "USD");

        // Act & Assert
        var act = () => orderItem.ApplyDiscount(negativeDiscount);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Discount cannot be negative");
    }

    [Fact]
    public void ApplyDiscount_ExceedingTotalValue_ShouldThrowException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var excessiveDiscount = new Money(250.00m, "USD");

        // Act & Assert
        var act = () => orderItem.ApplyDiscount(excessiveDiscount);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Discount cannot exceed total item value");
    }

    [Fact]
    public void RemoveDiscount_ShouldRemoveDiscount()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, _discount);

        // Act
        orderItem.RemoveDiscount();

        // Assert
        orderItem.Discount.Should().Be(Money.Zero("USD"));
        orderItem.TotalPrice.Amount.Should().Be(200.00m); // 2 * 100
    }

    [Fact]
    public void UpdateUnitPrice_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var newPrice = new Money(150.00m, "USD");

        // Act
        orderItem.UpdateUnitPrice(newPrice);

        // Assert
        orderItem.UnitPrice.Should().Be(newPrice);
        orderItem.TotalPrice.Amount.Should().Be(300.00m); // 2 * 150
    }

    [Fact]
    public void UpdateUnitPrice_WithNegativePrice_ShouldThrowException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var negativePrice = new Money(-50.00m, "USD");

        // Act & Assert
        var act = () => orderItem.UpdateUnitPrice(negativePrice);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Unit price cannot be negative");
    }

    [Fact]
    public void UpdateUnitPrice_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var priceDifferentCurrency = new Money(150.00m, "EUR");

        // Act & Assert
        var act = () => orderItem.UpdateUnitPrice(priceDifferentCurrency);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("New unit price currency must match current currency");
    }

    [Fact]
    public void UpdateUnitPrice_WhenDiscountExceedsNewTotal_ShouldThrowException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, new Money(150.00m, "USD"));
        var newLowerPrice = new Money(50.00m, "USD"); // New total would be 100, but discount is 150

        // Act & Assert
        var act = () => orderItem.UpdateUnitPrice(newLowerPrice);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Current discount exceeds new total item value");
    }

    [Fact]
    public void CalculateSavings_ShouldReturnDiscountAmount()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, _discount);

        // Act
        var savings = orderItem.CalculateSavings();

        // Assert
        savings.Should().Be(_discount);
    }

    [Fact]
    public void CalculateEffectiveUnitPrice_WithDiscount_ShouldReturnEffectivePrice()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, new Money(20.00m, "USD"));

        // Act
        var effectivePrice = orderItem.CalculateEffectiveUnitPrice();

        // Assert
        effectivePrice.Amount.Should().Be(90.00m); // 100 - (20/2)
    }

    [Fact]
    public void CalculateEffectiveUnitPrice_WithZeroQuantity_ShouldReturnUnitPrice()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 1, _unitPrice, _discount);
        
        // Use reflection to set quantity to 0 for testing edge case
        var quantityProperty = typeof(OrderItem).GetProperty("Quantity");
        quantityProperty!.SetValue(orderItem, 0);

        // Act
        var effectivePrice = orderItem.CalculateEffectiveUnitPrice();

        // Assert
        effectivePrice.Should().Be(_unitPrice);
    }

    [Fact]
    public void CalculateEffectiveUnitPrice_WithoutDiscount_ShouldReturnUnitPrice()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);

        // Act
        var effectivePrice = orderItem.CalculateEffectiveUnitPrice();

        // Assert
        effectivePrice.Should().Be(_unitPrice);
    }

    // Note: SetOrderId is an internal method used by the Order aggregate
    // and doesn't need to be tested directly as it's tested through Order aggregate tests

    [Fact]
    public void TotalPrice_WithMultipleQuantityAndDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var quantity = 3;
        var unitPrice = new Money(75.00m, "USD");
        var discount = new Money(25.00m, "USD");

        // Act
        var orderItem = OrderItem.Create(_productId, _productName, quantity, unitPrice, discount);

        // Assert
        orderItem.TotalPrice.Amount.Should().Be(200.00m); // (3 * 75) - 25 = 225 - 25 = 200
    }
}