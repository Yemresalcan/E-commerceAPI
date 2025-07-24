using ECommerce.Application.Commands.Products;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Products;

/// <summary>
/// Handler for DeleteProductCommand
/// </summary>
public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteProductCommandHandler> logger
) : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting product with ID: {ProductId}", request.ProductId);

        // Get the existing product
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{request.ProductId}' not found");
        }

        // Delete the product
        productRepository.Delete(product);

        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully deleted product with ID: {ProductId}", request.ProductId);

        return Unit.Value;
    }
}