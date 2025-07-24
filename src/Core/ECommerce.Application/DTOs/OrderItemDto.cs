namespace ECommerce.Application.DTOs;

/// <summary>
/// Data transfer object for order item information
/// </summary>
public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency,
    decimal Discount = 0
);