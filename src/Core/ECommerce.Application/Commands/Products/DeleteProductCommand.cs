namespace ECommerce.Application.Commands.Products;

/// <summary>
/// Command to delete a product
/// </summary>
public record DeleteProductCommand(Guid ProductId) : IRequest<Unit>;