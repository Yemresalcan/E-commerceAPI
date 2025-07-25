namespace ECommerce.Application.DTOs;

/// <summary>
/// Data transfer object for order information
/// </summary>
public record OrderDto(
    Guid Id,
    Guid CustomerId,
    CustomerSummaryDto Customer,
    string Status,
    string ShippingAddress,
    string BillingAddress,
    List<OrderItemDto> Items,
    PaymentDto? Payment,
    decimal TotalAmount,
    string Currency,
    int TotalItemCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ConfirmedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    string? CancellationReason
);

/// <summary>
/// Data transfer object for payment information
/// </summary>
public record PaymentDto(
    Guid Id,
    string Method,
    string Status,
    decimal Amount,
    string Currency,
    string? TransactionReference,
    DateTime? ProcessedAt
);

/// <summary>
/// Data transfer object for customer summary information
/// </summary>
public record CustomerSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber
);