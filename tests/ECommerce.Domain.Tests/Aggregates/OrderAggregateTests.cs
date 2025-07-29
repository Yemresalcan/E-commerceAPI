using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Events;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Tests.Aggregates;

public class OrderAggregateTests
{
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly string _shippingAddress = "123 Main St, City, State 12345";
    private readonly string _billingAddress = "456 Oak Ave, City, State 12345";
    private readonly Money _unitPrice = new(50m, "USD");

    private OrderItem CreateOrderItem(Guid? productId = null, int quantity = 2, Money? unitPrice = null)
    {
        return OrderItem.Create(
            productId ?? Guid.NewGuid(),
            "Test Product",
            quantity,
            unitPrice ?? _unitPrice
        );
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateOrder()
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems);

        // Assert
        order.Should().NotBeNull();
        order.CustomerId.Should().Be(_customerId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.ShippingAddress.Should().Be(_shippingAddress);
        order.BillingAddress.Should().Be(_billingAddress);
        order.OrderItems.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(100m); // 2 * 50
        order.TotalItemCount.Should().Be(2);
    }

    [Fact]
    public void Create_ShouldRaiseOrderPlacedEvent()
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems);

        // Assert
        var domainEvent = order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderPlacedEvent>().Subject;
        
        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.CustomerId.Should().Be(_customerId);
        domainEvent.TotalAmount.Should().Be(100m);
        domainEvent.Currency.Should().Be("USD");
        domainEvent.ItemCount.Should().Be(2);
        domainEvent.ShippingAddress.Should().Be(_shippingAddress);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowOrderDomainException()
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            Order.Create(Guid.Empty, _shippingAddress, _billingAddress, orderItems));
        
        exception.Message.Should().Contain("Customer ID cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidShippingAddress_ShouldThrowOrderDomainException(string invalidAddress)
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            Order.Create(_customerId, invalidAddress, _billingAddress, orderItems));
        
        exception.Message.Should().Contain("Shipping address is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidBillingAddress_ShouldThrowOrderDomainException(string invalidAddress)
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            Order.Create(_customerId, _shippingAddress, invalidAddress, orderItems));
        
        exception.Message.Should().Contain("Billing address is required");
    }

    [Fact]
    public void Create_WithNoOrderItems_ShouldThrowOrderDomainException()
    {
        // Arrange
        var orderItems = Array.Empty<OrderItem>();

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems));
        
        exception.Message.Should().Contain("Order must contain at least one item");
    }

    [Fact]
    public void Create_WithMixedCurrencies_ShouldThrowOrderDomainException()
    {
        // Arrange
        var orderItems = new[]
        {
            CreateOrderItem(unitPrice: new Money(50m, "USD")),
            CreateOrderItem(unitPrice: new Money(30m, "EUR"))
        };

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() =>
            Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems));
        
        exception.Message.Should().Contain("All order items must have the same currency");
    }

    [Fact]
    public void AddItem_ToPendingOrder_ShouldAddItem()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        var newItem = CreateOrderItem();

        // Act
        order.AddItem(newItem);

        // Assert
        order.OrderItems.Should().HaveCount(2);
    }

    [Fact]
    public void AddItem_WithSameProduct_ShouldCombineQuantities()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialItem = CreateOrderItem(productId, 2);
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { initialItem });
        var additionalItem = CreateOrderItem(productId, 3);

        // Act
        order.AddItem(additionalItem);

        // Assert
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_ToNonPendingOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.Confirm();
        var newItem = CreateOrderItem();

        // Act & Assert
        var exception = Assert.Throws<InvalidOrderStateException>(() => order.AddItem(newItem));
        exception.Message.Should().Contain("add items");
    }

    [Fact]
    public void AddItem_WithDifferentCurrency_ShouldThrowOrderDomainException()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        var itemWithDifferentCurrency = CreateOrderItem(unitPrice: new Money(30m, "EUR"));

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => order.AddItem(itemWithDifferentCurrency));
        exception.Message.Should().Contain("All order items must have the same currency");
    }

    [Fact]
    public void RemoveItem_FromPendingOrder_ShouldRemoveItem()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item1 = CreateOrderItem(productId);
        var item2 = CreateOrderItem();
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { item1, item2 });

        // Act
        order.RemoveItem(productId);

        // Assert
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.Should().NotContain(i => i.ProductId == productId);
    }

    [Fact]
    public void RemoveItem_NonExistentProduct_ShouldThrowOrderDomainException()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        var nonExistentProductId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => order.RemoveItem(nonExistentProductId));
        exception.Message.Should().Contain($"Item with product ID {nonExistentProductId} not found in order");
    }

    [Fact]
    public void RemoveItem_LastItem_ShouldThrowOrderDomainException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem(productId) });

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => order.RemoveItem(productId));
        exception.Message.Should().Contain("Order must contain at least one item");
    }

    [Fact]
    public void UpdateItemQuantity_WithValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem(productId, 2) });

        // Act
        order.UpdateItemQuantity(productId, 5);

        // Assert
        var item = order.OrderItems.First(i => i.ProductId == productId);
        item.Quantity.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateItemQuantity_WithInvalidQuantity_ShouldThrowOrderDomainException(int invalidQuantity)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem(productId) });

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => order.UpdateItemQuantity(productId, invalidQuantity));
        exception.Message.Should().Contain("Item quantity must be greater than zero");
    }

    [Fact]
    public void Confirm_PendingOrder_ShouldConfirmOrder()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_ShouldRaiseOrderConfirmedEvent()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.ClearDomainEvents();

        // Act
        order.Confirm();

        // Assert
        var domainEvent = order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderConfirmedEvent>().Subject;
        
        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.CustomerId.Should().Be(_customerId);
    }

    [Fact]
    public void Confirm_NonPendingOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.Confirm();

        // Act & Assert
        var exception = Assert.Throws<InvalidOrderStateException>(() => order.Confirm());
        exception.Message.Should().Contain("confirm");
    }

    [Fact]
    public void Ship_ConfirmedOrder_ShouldShipOrder()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.Confirm();

        // Act
        order.Ship();

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Ship_ShouldRaiseOrderShippedEvent()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.Confirm();
        order.ClearDomainEvents();

        // Act
        order.Ship();

        // Assert
        var domainEvent = order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderShippedEvent>().Subject;
        
        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.CustomerId.Should().Be(_customerId);
        domainEvent.ShippingAddress.Should().Be(_shippingAddress);
    }

    [Fact]
    public void Ship_NonConfirmedOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });

        // Act & Assert
        var exception = Assert.Throws<InvalidOrderStateException>(() => order.Ship());
        exception.Message.Should().Contain("ship");
    }

    [Fact]
    public void Deliver_ShippedOrder_ShouldDeliverOrder()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.Confirm();
        order.Ship();

        // Act
        order.Deliver();

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void Deliver_ShouldRaiseOrderDeliveredEvent()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.Confirm();
        order.Ship();
        order.ClearDomainEvents();

        // Act
        order.Deliver();

        // Assert
        var domainEvent = order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderDeliveredEvent>().Subject;
        
        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.CustomerId.Should().Be(_customerId);
    }

    [Fact]
    public void Cancel_WithReason_ShouldCancelOrder()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        var reason = "Customer requested cancellation";

        // Act
        order.Cancel(reason);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldRaiseOrderCancelledEvent()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.ClearDomainEvents();
        var reason = "Customer requested cancellation";

        // Act
        order.Cancel(reason);

        // Assert
        var domainEvent = order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCancelledEvent>().Subject;
        
        domainEvent.OrderId.Should().Be(order.Id);
        domainEvent.CustomerId.Should().Be(_customerId);
        domainEvent.Reason.Should().Be(reason);
    }

    [Fact]
    public void Cancel_DeliveredOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.Confirm();
        order.Ship();
        order.Deliver();

        // Act & Assert
        var exception = Assert.Throws<InvalidOrderStateException>(() => order.Cancel("reason"));
        exception.Message.Should().Contain("cancel");
    }

    [Fact]
    public void Cancel_AlreadyCancelledOrder_ShouldThrowOrderDomainException()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.Cancel("First cancellation");

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => order.Cancel("Second cancellation"));
        exception.Message.Should().Contain("Order is already cancelled");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_WithInvalidReason_ShouldThrowOrderDomainException(string invalidReason)
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });

        // Act & Assert
        var exception = Assert.Throws<OrderDomainException>(() => order.Cancel(invalidReason));
        exception.Message.Should().Contain("Cancellation reason is required");
    }

    [Fact]
    public void TotalAmount_WithMultipleItems_ShouldCalculateCorrectly()
    {
        // Arrange
        var item1 = CreateOrderItem(quantity: 2, unitPrice: new Money(50m, "USD"));
        var item2 = CreateOrderItem(quantity: 1, unitPrice: new Money(30m, "USD"));
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { item1, item2 });

        // Act
        var total = order.TotalAmount;

        // Assert
        total.Amount.Should().Be(130m); // (2 * 50) + (1 * 30)
        total.Currency.Should().Be("USD");
    }

    [Fact]
    public void TotalItemCount_WithMultipleItems_ShouldCalculateCorrectly()
    {
        // Arrange
        var item1 = CreateOrderItem(quantity: 2);
        var item2 = CreateOrderItem(quantity: 3);
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { item1, item2 });

        // Act
        var totalCount = order.TotalItemCount;

        // Assert
        totalCount.Should().Be(5);
    }

    [Fact]
    public void UpdateShippingAddress_PendingOrder_ShouldUpdateAddress()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        var newAddress = "789 New St, City, State 54321";

        // Act
        order.UpdateShippingAddress(newAddress);

        // Assert
        order.ShippingAddress.Should().Be(newAddress);
    }

    [Fact]
    public void UpdateShippingAddress_ShippedOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        order.Confirm();
        order.Ship();

        // Act & Assert
        var exception = Assert.Throws<InvalidOrderStateException>(() => 
            order.UpdateShippingAddress("New address"));
        exception.Message.Should().Contain("update shipping address");
    }

    [Fact]
    public void UpdateBillingAddress_PendingOrder_ShouldUpdateAddress()
    {
        // Arrange
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, new[] { CreateOrderItem() });
        var newAddress = "789 New Billing St, City, State 54321";

        // Act
        order.UpdateBillingAddress(newAddress);

        // Assert
        order.BillingAddress.Should().Be(newAddress);
    }
}