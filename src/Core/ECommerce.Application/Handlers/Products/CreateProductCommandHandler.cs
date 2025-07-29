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
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["ProductSku"] = request.Sku,
            ["ProductName"] = request.Name,
            ["CategoryId"] = request.CategoryId,
            ["Price"] = request.Price,
            ["Currency"] = request.Currency
        }))
        {
            logger.LogInformation("Starting product creation for SKU: {Sku}", request.Sku);

            // Check if SKU already exists
            logger.LogDebug("Checking if product with SKU {Sku} already exists", request.Sku);
            var existingProduct = await productRepository.GetBySkuAsync(request.Sku, cancellationToken);
            if (existingProduct != null)
            {
                logger.LogWarning("Product creation failed: SKU {Sku} already exists", request.Sku);
                throw new InvalidOperationException($"A product with SKU '{request.Sku}' already exists");
            }

            // Create the product
            logger.LogDebug("Creating product entity for SKU: {Sku}", request.Sku);
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
            logger.LogDebug("Adding product {ProductId} to repository", product.Id);
            await productRepository.AddAsync(product, cancellationToken);

            // Save changes
            logger.LogDebug("Saving changes for product {ProductId}", product.Id);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully created product {ProductId} with SKU: {Sku}", product.Id, request.Sku);

            return product.Id;
        }
    }
}