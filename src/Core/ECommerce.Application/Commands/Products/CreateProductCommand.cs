using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.Commands.Products;

/// <summary>
/// Command to create a new product
/// </summary>
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string Sku,
    int StockQuantity,
    int MinimumStockLevel,
    Guid CategoryId,
    decimal Weight = 0,
    string Dimensions = ""
) : IRequest<Guid>;