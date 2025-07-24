using ECommerce.Application.Commands.Products;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.Handlers.Products;

/// <summary>
/// Handler for CreateProductCommand
/// </summary>
public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateProductCommandHandler> logger
) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product with SKU: {Sku}", request.Sku);

        // Check if SKU already exists
        var existingProduct = await productRepository.GetBySkuAsync(request.Sku, cancellationToken);
        if (existingProduct != null)
        {
            throw new InvalidOperationException($"A product with SKU '{request.Sku}' already exists");
        }

        // Create the product
        var price = new Money(request.Price, request.Currency);
        var product = Product.Create(
            request.Name,
            request.Description,
            price,
            request.Sku,
            request.StockQuantity,
            request.MinimumStockLevel,
            request.CategoryId,
            request.Weight,
            request.Dimensions
        );

        // Add to repository
        await productRepository.AddAsync(product, cancellationToken);

        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created product with ID: {ProductId}", product.Id);

        return product.Id;
    }
}