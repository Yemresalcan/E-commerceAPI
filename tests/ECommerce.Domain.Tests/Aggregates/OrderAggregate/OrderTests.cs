using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Events;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;
using FluentAssertions;

namespace ECommerce.Domain.Tests.Aggregates.OrderAggregate;

public class OrderTests
{
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly string _shippingAddress = "123 Main St, City, State 12345";
    private readonly string _billingAddress = "456 Oak Ave, City, State 12345";

    private static OrderItem CreateOrderItem(
        Guid? productId = null,
        string productName = "Test Product",
        int quantity = 1,
        decimal unitPrice = 100.00m,
        string currency = "USD",
        Money? discount = null)
    {
        return OrderItem.Create(
            productId ?? Guid.NewGuid(),
            productName,
            quantity,
            new Money(unitPrice, currency),
            discount);
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(_customerId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.ShippingAddress.Should().Be(_shippingAddress);
        order.BillingAddress.Should().Be(_billingAddress);
        order.OrderItems.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(100.00m);
        order.TotalAmount.Currency.Should().Be("USD");
        order.TotalItemCount.Should().Be(1);
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseOrderPlacedEvent()
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems);

        // Assert
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderPlacedEvent>()
            .Which.Should().Match<OrderPlacedEvent>(e =>
                e.OrderId == order.Id &&
                e.CustomerId == _customerId &&
                e.TotalAmount == 100.00m &&
                e.Currency == "USD" &&
                e.ItemCount == 1 &&
                e.ShippingAddress == _shippingAddress);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowException()
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act & Assert
        var act = () => Order.Create(Guid.Empty, _shippingAddress, _billingAddress, orderItems);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Customer ID cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyShippingAddress_ShouldThrowException()
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act & Assert
        var act = () => Order.Create(_customerId, "", _billingAddress, orderItems);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Shipping address is required");
    }

    [Fact]
    public void Create_WithEmptyBillingAddress_ShouldThrowException()
    {
        // Arrange
        var orderItems = new[] { CreateOrderItem() };

        // Act & Assert
        var act = () => Order.Create(_customerId, _shippingAddress, "", orderItems);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Billing address is required");
    }

    [Fact]
    public void Create_WithNoItems_ShouldThrowException()
    {
        // Arrange
        var orderItems = Array.Empty<OrderItem>();

        // Act & Assert
        var act = () => Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Order must contain at least one item");
    }

    [Fact]
    public void Create_WithItemsOfDifferentCurrencies_ShouldThrowException()
    {
        // Arrange
        var orderItems = new[]
        {
            CreateOrderItem(currency: "USD"),
            CreateOrderItem(currency: "EUR")
        };

        // Act & Assert
        var act = () => Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("All order items must have the same currency");
    }

    [Fact]
    public void AddItem_ToPendingOrder_ShouldAddItem()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        var newItem = CreateOrderItem();

        // Act
        order.AddItem(newItem);

        // Assert
        order.OrderItems.Should().HaveCount(2);
        order.TotalItemCount.Should().Be(2);
        order.TotalAmount.Amount.Should().Be(200.00m);
    }

    [Fact]
    public void AddItem_WithSameProduct_ShouldUpdateQuantity()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialItems = new[] { CreateOrderItem(productId, quantity: 2) };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        var additionalItem = CreateOrderItem(productId, quantity: 3);

        // Act
        order.AddItem(additionalItem);

        // Assert
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().Quantity.Should().Be(5);
        order.TotalItemCount.Should().Be(5);
    }

    [Fact]
    public void AddItem_ToNonPendingOrder_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        order.Confirm();
        var newItem = CreateOrderItem();

        // Act & Assert
        var act = () => order.AddItem(newItem);
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage("Cannot add items order in Confirmed state");
    }

    [Fact]
    public void RemoveItem_FromPendingOrder_ShouldRemoveItem()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialItems = new[]
        {
            CreateOrderItem(productId),
            CreateOrderItem()
        };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);

        // Act
        order.RemoveItem(productId);

        // Assert
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.Should().NotContain(item => item.ProductId == productId);
    }

    [Fact]
    public void RemoveItem_LastItem_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialItems = new[] { CreateOrderItem(productId) };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);

        // Act & Assert
        var act = () => order.RemoveItem(productId);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Order must contain at least one item");
    }

    [Fact]
    public void RemoveItem_NonExistentProduct_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        var nonExistentProductId = Guid.NewGuid();

        // Act & Assert
        var act = () => order.RemoveItem(nonExistentProductId);
        act.Should().Throw<OrderDomainException>()
            .WithMessage($"Item with product ID {nonExistentProductId} not found in order");
    }

    [Fact]
    public void UpdateItemQuantity_ValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialItems = new[] { CreateOrderItem(productId, quantity: 2) };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);

        // Act
        order.UpdateItemQuantity(productId, 5);

        // Assert
        order.OrderItems.First().Quantity.Should().Be(5);
        order.TotalItemCount.Should().Be(5);
    }

    [Fact]
    public void UpdateItemQuantity_ZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialItems = new[] { CreateOrderItem(productId) };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);

        // Act & Assert
        var act = () => order.UpdateItemQuantity(productId, 0);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Item quantity must be greater than zero");
    }

    [Fact]
    public void Confirm_PendingOrder_ShouldConfirmAndRaiseEvent()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        order.ClearDomainEvents(); // Clear the OrderPlacedEvent

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderConfirmedEvent>();
    }

    [Fact]
    public void Confirm_NonPendingOrder_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        order.Confirm();

        // Act & Assert
        var act = () => order.Confirm();
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage("Cannot confirm order in Confirmed state");
    }

    [Fact]
    public void Ship_ConfirmedOrder_ShouldShipAndRaiseEvent()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        order.Confirm();
        order.ClearDomainEvents();

        // Act
        order.Ship();

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderShippedEvent>();
    }

    [Fact]
    public void Ship_NonConfirmedOrder_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);

        // Act & Assert
        var act = () => order.Ship();
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage("Cannot ship order in Pending state");
    }

    [Fact]
    public void Deliver_ShippedOrder_ShouldDeliverAndRaiseEvent()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        order.Confirm();
        order.Ship();
        order.ClearDomainEvents();

        // Act
        order.Deliver();

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderDeliveredEvent>();
    }

    [Fact]
    public void Deliver_NonShippedOrder_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        order.Confirm();

        // Act & Assert
        var act = () => order.Deliver();
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage("Cannot deliver order in Confirmed state");
    }

    [Fact]
    public void Cancel_NonDeliveredOrder_ShouldCancelAndRaiseEvent()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        order.ClearDomainEvents();
        var reason = "Customer requested cancellation";

        // Act
        order.Cancel(reason);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCancelledEvent>()
            .Which.Should().Match<OrderCancelledEvent>(e =>
                e.OrderId == order.Id &&
                e.CustomerId == _customerId &&
                e.Reason == reason);
    }

    [Fact]
    public void Cancel_DeliveredOrder_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        order.Confirm();
        order.Ship();
        order.Deliver();

        // Act & Assert
        var act = () => order.Cancel("Test reason");
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage("Cannot cancel order in Delivered state");
    }

    [Fact]
    public void Cancel_WithEmptyReason_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);

        // Act & Assert
        var act = () => order.Cancel("");
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Cancellation reason is required");
    }

    [Fact]
    public void AddPayment_ValidPayment_ShouldAddPayment()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        var payment = Payment.Create(100.00m, "USD", PaymentMethod.CreditCard);

        // Act
        order.AddPayment(payment);

        // Assert
        order.Payment.Should().NotBeNull();
        order.Payment.Should().Be(payment);
    }

    [Fact]
    public void AddPayment_MismatchedAmount_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        var payment = Payment.Create(50.00m, "USD", PaymentMethod.CreditCard);

        // Act & Assert
        var act = () => order.AddPayment(payment);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Payment amount must match order total");
    }

    [Fact]
    public void AddPayment_WhenPaymentAlreadyExists_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        var payment1 = Payment.Create(100.00m, "USD", PaymentMethod.CreditCard);
        var payment2 = Payment.Create(100.00m, "USD", PaymentMethod.PayPal);
        order.AddPayment(payment1);

        // Act & Assert
        var act = () => order.AddPayment(payment2);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Order already has payment information");
    }

    [Fact]
    public void UpdateShippingAddress_PendingOrder_ShouldUpdateAddress()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        var newAddress = "789 New St, City, State 12345";

        // Act
        order.UpdateShippingAddress(newAddress);

        // Assert
        order.ShippingAddress.Should().Be(newAddress);
    }

    [Fact]
    public void UpdateShippingAddress_ShippedOrder_ShouldThrowException()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        order.Confirm();
        order.Ship();

        // Act & Assert
        var act = () => order.UpdateShippingAddress("New Address");
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage("Cannot update shipping address order in Shipped state");
    }

    [Fact]
    public void UpdateBillingAddress_PendingOrder_ShouldUpdateAddress()
    {
        // Arrange
        var initialItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, initialItems);
        var newAddress = "789 New St, City, State 12345";

        // Act
        order.UpdateBillingAddress(newAddress);

        // Assert
        order.BillingAddress.Should().Be(newAddress);
    }

    [Fact]
    public void TotalAmount_WithMultipleItems_ShouldCalculateCorrectly()
    {
        // Arrange
        var orderItems = new[]
        {
            CreateOrderItem(quantity: 2, unitPrice: 50.00m),
            CreateOrderItem(quantity: 1, unitPrice: 75.00m)
        };

        // Act
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems);

        // Assert
        order.TotalAmount.Amount.Should().Be(175.00m); // (2 * 50) + (1 * 75)
        order.TotalItemCount.Should().Be(3);
    }

    [Fact]
    public void TotalAmount_WithNoItems_ShouldReturnZero()
    {
        // This test verifies the property behavior, though creating an order without items would throw
        // We'll test this through reflection or by examining the property logic
        var orderItems = new[] { CreateOrderItem() };
        var order = Order.Create(_customerId, _shippingAddress, _billingAddress, orderItems);
        
        // Remove the item using reflection to test the TotalAmount property with empty items
        var itemsField = typeof(Order).GetField("_orderItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var items = (List<OrderItem>)itemsField!.GetValue(order)!;
        items.Clear();

        // Act & Assert
        order.TotalAmount.Should().Be(Money.Zero("USD"));
        order.TotalItemCount.Should().Be(0);
    }
}