namespace ECommerce.Application.Commands.Products;

/// <summary>
/// Command to update an existing product
/// </summary>
public record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    decimal Weight = 0,
    string Dimensions = ""
) : IRequest<Unit>;