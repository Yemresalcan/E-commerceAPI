namespace ECommerce.Application.Commands.Products;

/// <summary>
/// Command to update product stock quantity
/// </summary>
public record UpdateProductStockCommand(
    Guid ProductId,
    int StockQuantity,
    string Reason = "Manual stock update") : IRequest;