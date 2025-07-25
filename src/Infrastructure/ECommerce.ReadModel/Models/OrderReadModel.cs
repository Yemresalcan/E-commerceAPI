using Nest;

namespace ECommerce.ReadModel.Models;

/// <summary>
/// Read model for Order optimized for search and query operations
/// </summary>
[ElasticsearchType(IdProperty = nameof(Id))]
public class OrderReadModel
{
    /// <summary>
    /// Order unique identifier
    /// </summary>
    [Keyword]
    public Guid Id { get; set; }

    /// <summary>
    /// Customer who placed the order
    /// </summary>
    [Keyword]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Customer information
    /// </summary>
    [Object]
    public CustomerSummaryReadModel Customer { get; set; } = new();

    /// <summary>
    /// Current order status
    /// </summary>
    [Keyword]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address
    /// </summary>
    [Text]
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Billing address
    /// </summary>
    [Text]
    public string BillingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Order items
    /// </summary>
    [Object]
    public List<OrderItemReadModel> Items { get; set; } = [];

    /// <summary>
    /// Payment information
    /// </summary>
    [Object]
    public PaymentReadModel? Payment { get; set; }

    /// <summary>
    /// Total order amount
    /// </summary>
    [Number(NumberType.Double)]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Order currency
    /// </summary>
    [Keyword]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Total number of items in the order
    /// </summary>
    [Number(NumberType.Integer)]
    public int TotalItemCount { get; set; }

    /// <summary>
    /// Order creation date
    /// </summary>
    [Date]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Order last update date
    /// </summary>
    [Date]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Date when order was confirmed
    /// </summary>
    [Date]
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Date when order was shipped
    /// </summary>
    [Date]
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Date when order was delivered
    /// </summary>
    [Date]
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Date when order was cancelled
    /// </summary>
    [Date]
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Cancellation reason if applicable
    /// </summary>
    [Text]
    public string? CancellationReason { get; set; }
}

/// <summary>
/// Order item information for order read model
/// </summary>
public class OrderItemReadModel
{
    /// <summary>
    /// Order item unique identifier
    /// </summary>
    [Keyword]
    public Guid Id { get; set; }

    /// <summary>
    /// Product identifier
    /// </summary>
    [Keyword]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product name at time of order
    /// </summary>
    [Text]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product SKU at time of order
    /// </summary>
    [Keyword]
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Quantity ordered
    /// </summary>
    [Number(NumberType.Integer)]
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price at time of order
    /// </summary>
    [Number(NumberType.Double)]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Currency for unit price
    /// </summary>
    [Keyword]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Total price for this item (quantity * unit price)
    /// </summary>
    [Number(NumberType.Double)]
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// Payment information for order read model
/// </summary>
public class PaymentReadModel
{
    /// <summary>
    /// Payment unique identifier
    /// </summary>
    [Keyword]
    public Guid Id { get; set; }

    /// <summary>
    /// Payment method used
    /// </summary>
    [Keyword]
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Payment status
    /// </summary>
    [Keyword]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Payment amount
    /// </summary>
    [Number(NumberType.Double)]
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment currency
    /// </summary>
    [Keyword]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// External transaction reference
    /// </summary>
    [Keyword]
    public string? TransactionReference { get; set; }

    /// <summary>
    /// Payment processing date
    /// </summary>
    [Date]
    public DateTime? ProcessedAt { get; set; }
}

/// <summary>
/// Customer summary information for order read model
/// </summary>
public class CustomerSummaryReadModel
{
    /// <summary>
    /// Customer unique identifier
    /// </summary>
    [Keyword]
    public Guid Id { get; set; }

    /// <summary>
    /// Customer full name
    /// </summary>
    [Text]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Customer email
    /// </summary>
    [Keyword]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone number
    /// </summary>
    [Keyword]
    public string? PhoneNumber { get; set; }
}