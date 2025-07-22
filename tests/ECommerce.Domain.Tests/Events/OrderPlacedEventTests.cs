using ECommerce.Domain.Events;

namespace ECommerce.Domain.Tests.Events;

public class OrderPlacedEventTests
{
    [Fact]
    public void OrderPlacedEvent_Should_Initialize_All_Properties_Correctly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var totalAmount = 299.99m;
        var currency = "USD";
        var itemCount = 3;
        var shippingAddress = "123 Main St, City, State 12345";

        // Act
        var orderPlacedEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            totalAmount,
            currency,
            itemCount,
            shippingAddress);

        // Assert
        orderPlacedEvent.OrderId.Should().Be(orderId);
        orderPlacedEvent.CustomerId.Should().Be(customerId);
        orderPlacedEvent.TotalAmount.Should().Be(totalAmount);
        orderPlacedEvent.Currency.Should().Be(currency);
        orderPlacedEvent.ItemCount.Should().Be(itemCount);
        orderPlacedEvent.ShippingAddress.Should().Be(shippingAddress);
        orderPlacedEvent.Id.Should().NotBe(Guid.Empty);
        orderPlacedEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        orderPlacedEvent.Version.Should().Be(1);
    }

    [Fact]
    public void OrderPlacedEvent_Should_Inherit_From_DomainEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Act
        var orderPlacedEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            299.99m,
            "USD",
            3,
            "123 Main St");

        // Assert
        orderPlacedEvent.Should().BeAssignableTo<DomainEvent>();
    }

    [Fact]
    public void OrderPlacedEvent_Should_Support_Record_Equality()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var event1 = new OrderPlacedEvent(
            orderId,
            customerId,
            299.99m,
            "USD",
            3,
            "123 Main St");

        var event2 = new OrderPlacedEvent(
            orderId,
            customerId,
            299.99m,
            "USD",
            3,
            "123 Main St");

        // Act & Assert
        event1.Should().NotBe(event2); // Different instances have different IDs and OccurredOn
        event1.OrderId.Should().Be(event2.OrderId);
        event1.CustomerId.Should().Be(event2.CustomerId);
        event1.TotalAmount.Should().Be(event2.TotalAmount);
        event1.Currency.Should().Be(event2.Currency);
        event1.ItemCount.Should().Be(event2.ItemCount);
        event1.ShippingAddress.Should().Be(event2.ShippingAddress);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(99.99)]
    [InlineData(1000.00)]
    [InlineData(9999999.99)]
    public void OrderPlacedEvent_Should_Accept_Various_Total_Amounts(decimal totalAmount)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Act
        var orderPlacedEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            totalAmount,
            "USD",
            1,
            "123 Main St");

        // Assert
        orderPlacedEvent.TotalAmount.Should().Be(totalAmount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void OrderPlacedEvent_Should_Accept_Various_Item_Counts(int itemCount)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Act
        var orderPlacedEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            299.99m,
            "USD",
            itemCount,
            "123 Main St");

        // Assert
        orderPlacedEvent.ItemCount.Should().Be(itemCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123 Main St")]
    [InlineData("Very Long Address With Multiple Lines\n456 Second St\nApt 789\nCity, State 12345")]
    public void OrderPlacedEvent_Should_Accept_Various_Shipping_Addresses(string shippingAddress)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Act
        var orderPlacedEvent = new OrderPlacedEvent(
            orderId,
            customerId,
            299.99m,
            "USD",
            1,
            shippingAddress);

        // Assert
        orderPlacedEvent.ShippingAddress.Should().Be(shippingAddress);
    }
}