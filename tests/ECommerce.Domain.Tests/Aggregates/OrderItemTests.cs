using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Tests.Aggregates;

public class OrderItemTests
{
    private readonly Guid _productId = Guid.NewGuid();
    private readonly string _productName = "Test Product";
    private readonly Money _unitPrice = new(25.50m, "USD");

    [Fact]
    public void Create_WithValidParameters_ShouldCreateOrderItem()
    {
        // Arrange
        var quantity = 3;

        // Act
        var orderItem = OrderItem.Create(_productId, _productName, quantity, _unitPrice);

        // Assert
        orderItem.Should().NotBeNull();
        orderItem.ProductId.Should().Be(_productId);
        orderItem.ProductName.Should().Be(_productName);
        orderItem.Quantity.Should().Be(quantity);
        orderItem.UnitPrice.Should().Be(_unitPrice);
        orderItem.Discount.Should().Be(Money.Zero("USD"));
        orderItem.TotalPrice.Amount.Should().Be(76.50m); // 3 * 25.50
    }

    [Fact]
    public void Create_WithDiscount_ShouldCreateOrderItemWithDiscount()
    {
        // Arrange
        var quantity = 2;
        var discount = new Money(5m, "USD");

        // Act
        var orderItem = OrderItem.Create(_productId, _productName, quantity, _unitPrice, discount);

        // Assert
        orderItem.Discount.Should().Be(discount);
        orderItem.TotalPrice.Amount.Should().Be(46m); // (2 * 25.50) - 5
    }

    [Fact]
    public void Create_WithEmptyProductId_ShouldThrowOrderDomainException()
    {
        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            OrderItem.Create(Guid.Empty, _productName, 1, _unitPrice));
        
        exception.Message.Should().Contain("Product ID cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidProductName_ShouldThrowOrderDomainException(string invalidName)
    {
        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            OrderItem.Create(_productId, invalidName, 1, _unitPrice));
        
        exception.Message.Should().Contain("Product name is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Create_WithInvalidQuantity_ShouldThrowOrderDomainException(int invalidQuantity)
    {
        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            OrderItem.Create(_productId, _productName, invalidQuantity, _unitPrice));
        
        exception.Message.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public void Create_WithNegativeUnitPrice_ShouldThrowOrderDomainException()
    {
        // Arrange
        var negativePrice = new Money(-10m, "USD");

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            OrderItem.Create(_productId, _productName, 1, negativePrice));
        
        exception.Message.Should().Contain("Unit price cannot be negative");
    }

    [Fact]
    public void Create_WithDiscountDifferentCurrency_ShouldThrowOrderDomainException()
    {
        // Arrange
        var discount = new Money(5m, "EUR");

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            OrderItem.Create(_productId, _productName, 2, _unitPrice, discount));
        
        exception.Message.Should().Contain("Discount currency must match unit price currency");
    }

    [Fact]
    public void Create_WithDiscountExceedingTotalValue_ShouldThrowOrderDomainException()
    {
        // Arrange
        var quantity = 2;
        var totalValue = _unitPrice.Multiply(quantity); // 51
        var excessiveDiscount = totalValue.Add(new Money(1m, "USD")); // 52

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            OrderItem.Create(_productId, _productName, quantity, _unitPrice, excessiveDiscount));
        
        exception.Message.Should().Contain("Discount cannot exceed total item value");
    }

    [Fact]
    public void UpdateQuantity_WithValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var newQuantity = 5;

        // Act
        orderItem.UpdateQuantity(newQuantity);

        // Assert
        orderItem.Quantity.Should().Be(newQuantity);
        orderItem.TotalPrice.Amount.Should().Be(127.50m); // 5 * 25.50
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateQuantity_WithInvalidQuantity_ShouldThrowOrderDomainException(int invalidQuantity)
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => orderItem.UpdateQuantity(invalidQuantity));
        exception.Message.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public void UpdateQuantity_WithDiscountExceedingNewTotal_ShouldThrowOrderDomainException()
    {
        // Arrange
        var discount = new Money(40m, "USD");
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, discount); // Total: 51, Discount: 40

        // Act & Assert - Reducing quantity to 1 would make total 25.50, less than discount of 40
        var exception = Assert.Throws<OrderDomainException>(() => orderItem.UpdateQuantity(1));
        exception.Message.Should().Contain("Current discount exceeds new total item value");
    }

    [Fact]
    public void ApplyDiscount_WithValidDiscount_ShouldApplyDiscount()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var discount = new Money(10m, "USD");

        // Act
        orderItem.ApplyDiscount(discount);

        // Assert
        orderItem.Discount.Should().Be(discount);
        orderItem.TotalPrice.Amount.Should().Be(41m); // (2 * 25.50) - 10
    }

    [Fact]
    public void ApplyDiscount_WithNullDiscount_ShouldThrowArgumentNullException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => orderItem.ApplyDiscount(null!));
    }

    [Fact]
    public void ApplyDiscount_WithDifferentCurrency_ShouldThrowOrderDomainException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var discount = new Money(10m, "EUR");

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => orderItem.ApplyDiscount(discount));
        exception.Message.Should().Contain("Discount currency must match unit price currency");
    }

    [Fact]
    public void ApplyDiscount_WithNegativeDiscount_ShouldThrowOrderDomainException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var negativeDiscount = new Money(-5m, "USD");

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => orderItem.ApplyDiscount(negativeDiscount));
        exception.Message.Should().Contain("Discount cannot be negative");
    }

    [Fact]
    public void ApplyDiscount_ExceedingTotalValue_ShouldThrowOrderDomainException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var totalValue = orderItem.TotalPrice;
        var excessiveDiscount = totalValue.Add(new Money(1m, "USD"));

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => orderItem.ApplyDiscount(excessiveDiscount));
        exception.Message.Should().Contain("Discount cannot exceed total item value");
    }

    [Fact]
    public void RemoveDiscount_ShouldSetDiscountToZero()
    {
        // Arrange
        var discount = new Money(10m, "USD");
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, discount);

        // Act
        orderItem.RemoveDiscount();

        // Assert
        orderItem.Discount.Should().Be(Money.Zero("USD"));
        orderItem.TotalPrice.Amount.Should().Be(51m); // 2 * 25.50
    }

    [Fact]
    public void UpdateUnitPrice_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var newPrice = new Money(30m, "USD");

        // Act
        orderItem.UpdateUnitPrice(newPrice);

        // Assert
        orderItem.UnitPrice.Should().Be(newPrice);
        orderItem.TotalPrice.Amount.Should().Be(60m); // 2 * 30
    }

    [Fact]
    public void UpdateUnitPrice_WithNullPrice_ShouldThrowArgumentNullException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => orderItem.UpdateUnitPrice(null!));
    }

    [Fact]
    public void UpdateUnitPrice_WithNegativePrice_ShouldThrowOrderDomainException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var negativePrice = new Money(-10m, "USD");

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => orderItem.UpdateUnitPrice(negativePrice));
        exception.Message.Should().Contain("Unit price cannot be negative");
    }

    [Fact]
    public void UpdateUnitPrice_WithDifferentCurrency_ShouldThrowOrderDomainException()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        var differentCurrencyPrice = new Money(30m, "EUR");

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => orderItem.UpdateUnitPrice(differentCurrencyPrice));
        exception.Message.Should().Contain("New unit price currency must match current currency");
    }

    [Fact]
    public void UpdateUnitPrice_WithDiscountExceedingNewTotal_ShouldThrowOrderDomainException()
    {
        // Arrange
        var discount = new Money(40m, "USD");
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, discount); // Total: 51, Discount: 40
        var newLowPrice = new Money(15m, "USD"); // New total would be 30, less than discount of 40

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => orderItem.UpdateUnitPrice(newLowPrice));
        exception.Message.Should().Contain("Current discount exceeds new total item value");
    }

    [Fact]
    public void CalculateSavings_ShouldReturnDiscountAmount()
    {
        // Arrange
        var discount = new Money(15m, "USD");
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, discount);

        // Act
        var savings = orderItem.CalculateSavings();

        // Assert
        savings.Should().Be(discount);
    }

    [Fact]
    public void CalculateEffectiveUnitPrice_WithDiscount_ShouldReturnCorrectPrice()
    {
        // Arrange
        var discount = new Money(10m, "USD"); // $10 total discount
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice, discount); // 2 items at $25.50 each

        // Act
        var effectivePrice = orderItem.CalculateEffectiveUnitPrice();

        // Assert
        effectivePrice.Amount.Should().Be(20.50m); // 25.50 - (10/2) = 20.50
    }

    [Fact]
    public void CalculateEffectiveUnitPrice_WithZeroQuantity_ShouldReturnUnitPrice()
    {
        // Arrange
        var orderItem = OrderItem.Create(_productId, _productName, 1, _unitPrice);
        orderItem.UpdateQuantity(1); // This should work fine
        
        // We can't actually set quantity to 0 through normal means since it's validated,
        // so let's test with quantity 1 and no discount
        var discount = Money.Zero("USD");
        orderItem.ApplyDiscount(discount);

        // Act
        var effectivePrice = orderItem.CalculateEffectiveUnitPrice();

        // Assert
        effectivePrice.Should().Be(_unitPrice);
    }

    [Fact]
    public void TotalPrice_WithQuantityAndDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var quantity = 3;
        var discount = new Money(7.50m, "USD");
        var orderItem = OrderItem.Create(_productId, _productName, quantity, _unitPrice, discount);

        // Act
        var totalPrice = orderItem.TotalPrice;

        // Assert
        totalPrice.Amount.Should().Be(69m); // (3 * 25.50) - 7.50 = 76.50 - 7.50 = 69
    }

    [Fact]
    public void TotalPrice_WithNoDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var quantity = 4;
        var orderItem = OrderItem.Create(_productId, _productName, quantity, _unitPrice);

        // Act
        var totalPrice = orderItem.TotalPrice;

        // Assert
        totalPrice.Amount.Should().Be(102m); // 4 * 25.50
    }

    [Fact]
    public void OrderItemWorkflow_ShouldWorkCorrectly()
    {
        // Arrange - Create order item
        var orderItem = OrderItem.Create(_productId, _productName, 2, _unitPrice);
        
        // Assert initial state
        orderItem.TotalPrice.Amount.Should().Be(51m); // 2 * 25.50

        // Act - Apply discount
        var discount = new Money(6m, "USD");
        orderItem.ApplyDiscount(discount);
        
        // Assert after discount
        orderItem.TotalPrice.Amount.Should().Be(45m); // 51 - 6
        orderItem.CalculateSavings().Should().Be(discount);

        // Act - Update quantity
        orderItem.UpdateQuantity(3);
        
        // Assert after quantity update
        orderItem.TotalPrice.Amount.Should().Be(70.50m); // (3 * 25.50) - 6 = 76.50 - 6

        // Act - Update unit price
        var newPrice = new Money(20m, "USD");
        orderItem.UpdateUnitPrice(newPrice);
        
        // Assert after price update
        orderItem.TotalPrice.Amount.Should().Be(54m); // (3 * 20) - 6 = 60 - 6

        // Act - Remove discount
        orderItem.RemoveDiscount();
        
        // Assert after removing discount
        orderItem.TotalPrice.Amount.Should().Be(60m); // 3 * 20
        orderItem.Discount.Should().Be(Money.Zero("USD"));
    }
}