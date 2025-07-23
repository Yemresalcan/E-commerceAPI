using ECommerce.Domain.Events;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Order aggregate root that manages the order lifecycle and business rules
/// </summary>
public class Order : AggregateRoot
{
    private readonly List<OrderItem> _orderItems = [];
    private Payment? _payment;

    /// <summary>
    /// The customer who placed the order
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Current status of the order
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Shipping address for the order
    /// </summary>
    public string ShippingAddress { get; private set; } = string.Empty;

    /// <summary>
    /// Billing address for the order
    /// </summary>
    public string BillingAddress { get; private set; } = string.Empty;

    /// <summary>
    /// Read-only collection of order items
    /// </summary>
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    /// <summary>
    /// Payment information for the order
    /// </summary>
    public Payment? Payment => _payment;

    /// <summary>
    /// Calculated total amount of the order
    /// </summary>
    public Money TotalAmount
    {
        get
        {
            if (!_orderItems.Any())
                return Money.Zero("USD");

            var currency = _orderItems.First().UnitPrice.Currency;
            return _orderItems.Aggregate(
                Money.Zero(currency),
                (total, item) => total.Add(item.TotalPrice)
            );
        }
    }

    /// <summary>
    /// Total number of items in the order
    /// </summary>
    public int TotalItemCount => _orderItems.Sum(item => item.Quantity);

    // Private constructor for EF Core
    private Order() { }

    /// <summary>
    /// Creates a new order
    /// </summary>
    public static Order Create(
        Guid customerId,
        string shippingAddress,
        string billingAddress,
        IEnumerable<OrderItem> orderItems)
    {
        if (customerId == Guid.Empty)
            throw new OrderDomainException("Customer ID cannot be empty");

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new OrderDomainException("Shipping address is required");

        if (string.IsNullOrWhiteSpace(billingAddress))
            throw new OrderDomainException("Billing address is required");

        var items = orderItems.ToList();
        if (!items.Any())
            throw new OrderDomainException("Order must contain at least one item");

        // Validate all items have the same currency
        var currencies = items.Select(item => item.UnitPrice.Currency).Distinct().ToList();
        if (currencies.Count > 1)
            throw new OrderDomainException("All order items must have the same currency");

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress
        };

        foreach (var item in items)
        {
            order._orderItems.Add(item);
        }

        order.AddDomainEvent(new OrderPlacedEvent(
            order.Id,
            order.CustomerId,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.TotalItemCount,
            order.ShippingAddress
        ));

        return order;
    }

    /// <summary>
    /// Adds an item to the order
    /// </summary>
    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException(Status, "add items");

        ArgumentNullException.ThrowIfNull(item);

        // Check if item with same product already exists
        var existingItem = _orderItems.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + item.Quantity);
        }
        else
        {
            // Validate currency consistency
            if (_orderItems.Any() && _orderItems.First().UnitPrice.Currency != item.UnitPrice.Currency)
                throw new OrderDomainException("All order items must have the same currency");

            _orderItems.Add(item);
        }

        MarkAsModified();
    }

    /// <summary>
    /// Removes an item from the order
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException(Status, "remove items");

        var item = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (item == null)
            throw new OrderDomainException($"Item with product ID {productId} not found in order");

        _orderItems.Remove(item);

        if (!_orderItems.Any())
            throw new OrderDomainException("Order must contain at least one item");

        MarkAsModified();
    }

    /// <summary>
    /// Updates the quantity of an existing item
    /// </summary>
    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException(Status, "update item quantities");

        if (newQuantity <= 0)
            throw new OrderDomainException("Item quantity must be greater than zero");

        var item = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (item == null)
            throw new OrderDomainException($"Item with product ID {productId} not found in order");

        item.UpdateQuantity(newQuantity);
        MarkAsModified();
    }

    /// <summary>
    /// Confirms the order and changes status to confirmed
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException(Status, "confirm");

        if (!_orderItems.Any())
            throw new OrderDomainException("Cannot confirm order without items");

        Status = OrderStatus.Confirmed;
        MarkAsModified();

        AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId, TotalAmount.Amount, TotalAmount.Currency));
    }

    /// <summary>
    /// Ships the order and changes status to shipped
    /// </summary>
    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOrderStateException(Status, "ship");

        Status = OrderStatus.Shipped;
        MarkAsModified();

        AddDomainEvent(new OrderShippedEvent(Id, CustomerId, ShippingAddress));
    }

    /// <summary>
    /// Delivers the order and changes status to delivered
    /// </summary>
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOrderStateException(Status, "deliver");

        Status = OrderStatus.Delivered;
        MarkAsModified();

        AddDomainEvent(new OrderDeliveredEvent(Id, CustomerId));
    }

    /// <summary>
    /// Cancels the order
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOrderStateException(Status, "cancel");

        if (Status == OrderStatus.Cancelled)
            throw new OrderDomainException("Order is already cancelled");

        if (string.IsNullOrWhiteSpace(reason))
            throw new OrderDomainException("Cancellation reason is required");

        Status = OrderStatus.Cancelled;
        MarkAsModified();

        AddDomainEvent(new OrderCancelledEvent(Id, CustomerId, reason));
    }

    /// <summary>
    /// Adds payment information to the order
    /// </summary>
    public void AddPayment(Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        if (_payment != null)
            throw new OrderDomainException("Order already has payment information");

        // Validate payment amount matches order total
        var paymentMoney = new Money(payment.Amount, payment.Currency);
        if (!paymentMoney.Equals(TotalAmount))
            throw new OrderDomainException("Payment amount must match order total");

        _payment = payment;
        MarkAsModified();
    }

    /// <summary>
    /// Updates the shipping address
    /// </summary>
    public void UpdateShippingAddress(string newAddress)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Confirmed)
            throw new InvalidOrderStateException(Status, "update shipping address");

        if (string.IsNullOrWhiteSpace(newAddress))
            throw new OrderDomainException("Shipping address cannot be empty");

        ShippingAddress = newAddress;
        MarkAsModified();
    }

    /// <summary>
    /// Updates the billing address
    /// </summary>
    public void UpdateBillingAddress(string newAddress)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Confirmed)
            throw new InvalidOrderStateException(Status, "update billing address");

        if (string.IsNullOrWhiteSpace(newAddress))
            throw new OrderDomainException("Billing address cannot be empty");

        BillingAddress = newAddress;
        MarkAsModified();
    }
}