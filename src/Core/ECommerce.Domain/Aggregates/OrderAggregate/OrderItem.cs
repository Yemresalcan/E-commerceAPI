using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Represents an individual item within an order with quantity and pricing logic
/// </summary>
public class OrderItem : Entity
{
    /// <summary>
    /// The product identifier for this order item
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// The product name at the time of order (for historical purposes)
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// The quantity of the product ordered
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// The unit price of the product at the time of order
    /// </summary>
    public Money UnitPrice { get; private set; } = Money.Zero("USD");

    /// <summary>
    /// Any discount applied to this item
    /// </summary>
    public Money Discount { get; private set; } = Money.Zero("USD");

    /// <summary>
    /// Calculated total price for this item (quantity * unit price - discount)
    /// </summary>
    public Money TotalPrice => UnitPrice.Multiply(Quantity).Subtract(Discount);

    /// <summary>
    /// The order this item belongs to
    /// </summary>
    public Guid OrderId { get; private set; }

    // Private constructor for EF Core
    private OrderItem() { }

    /// <summary>
    /// Creates a new order item
    /// </summary>
    public static OrderItem Create(
        Guid productId,
        string productName,
        int quantity,
        Money unitPrice,
        Money? discount = null)
    {
        if (productId == Guid.Empty)
            throw new OrderDomainException("Product ID cannot be empty");

        if (string.IsNullOrWhiteSpace(productName))
            throw new OrderDomainException("Product name is required");

        if (quantity <= 0)
            throw new OrderDomainException("Quantity must be greater than zero");

        if (unitPrice.IsNegative)
            throw new OrderDomainException("Unit price cannot be negative");

        var discountAmount = discount ?? Money.Zero(unitPrice.Currency);

        // Validate discount currency matches unit price currency
        if (discountAmount.Currency != unitPrice.Currency)
            throw new OrderDomainException("Discount currency must match unit price currency");

        // Validate discount doesn't exceed total item value
        var totalItemValue = unitPrice.Multiply(quantity);
        if (discountAmount.IsGreaterThan(totalItemValue))
            throw new OrderDomainException("Discount cannot exceed total item value");

        return new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Discount = discountAmount
        };
    }

    /// <summary>
    /// Updates the quantity of this order item
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new OrderDomainException("Quantity must be greater than zero");

        // Validate discount doesn't exceed new total item value
        var newTotalItemValue = UnitPrice.Multiply(newQuantity);
        if (Discount.IsGreaterThan(newTotalItemValue))
            throw new OrderDomainException("Current discount exceeds new total item value");

        Quantity = newQuantity;
        MarkAsModified();
    }

    /// <summary>
    /// Applies a discount to this order item
    /// </summary>
    public void ApplyDiscount(Money discount)
    {
        ArgumentNullException.ThrowIfNull(discount);

        if (discount.Currency != UnitPrice.Currency)
            throw new OrderDomainException("Discount currency must match unit price currency");

        if (discount.IsNegative)
            throw new OrderDomainException("Discount cannot be negative");

        var totalItemValue = UnitPrice.Multiply(Quantity);
        if (discount.IsGreaterThan(totalItemValue))
            throw new OrderDomainException("Discount cannot exceed total item value");

        Discount = discount;
        MarkAsModified();
    }

    /// <summary>
    /// Removes any applied discount
    /// </summary>
    public void RemoveDiscount()
    {
        Discount = Money.Zero(UnitPrice.Currency);
        MarkAsModified();
    }

    /// <summary>
    /// Updates the unit price (typically used for price adjustments before order confirmation)
    /// </summary>
    public void UpdateUnitPrice(Money newUnitPrice)
    {
        ArgumentNullException.ThrowIfNull(newUnitPrice);

        if (newUnitPrice.IsNegative)
            throw new OrderDomainException("Unit price cannot be negative");

        if (newUnitPrice.Currency != UnitPrice.Currency)
            throw new OrderDomainException("New unit price currency must match current currency");

        // Validate discount doesn't exceed new total item value
        var newTotalItemValue = newUnitPrice.Multiply(Quantity);
        if (Discount.IsGreaterThan(newTotalItemValue))
            throw new OrderDomainException("Current discount exceeds new total item value");

        UnitPrice = newUnitPrice;
        MarkAsModified();
    }

    /// <summary>
    /// Calculates the savings from the applied discount
    /// </summary>
    public Money CalculateSavings()
    {
        return Discount;
    }

    /// <summary>
    /// Calculates the effective unit price after discount
    /// </summary>
    public Money CalculateEffectiveUnitPrice()
    {
        if (Quantity == 0)
            return UnitPrice;

        var discountPerUnit = Discount.Divide(Quantity);
        return UnitPrice.Subtract(discountPerUnit);
    }

    /// <summary>
    /// Sets the order ID (used internally by the Order aggregate)
    /// </summary>
    internal void SetOrderId(Guid orderId)
    {
        OrderId = orderId;
    }
}