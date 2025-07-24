using ECommerce.Application.Commands.Products;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.Handlers.Products;

/// <summary>
/// Handler for UpdateProductCommand
/// </summary>
public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProductCommandHandler> logger
) : IRequestHandler<UpdateProductCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating product with ID: {ProductId}", request.ProductId);

        // Get the existing product
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{request.ProductId}' not found");
        }

        // Update the product
        var price = new Money(request.Price, request.Currency);
        product.Update(
            request.Name,
            request.Description,
            price,
            request.Weight,
            request.Dimensions
        );

        // Update in repository
        productRepository.Update(product);

        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated product with ID: {ProductId}", request.ProductId);

        return Unit.Value;
    }
}