using ECommerce.Application.Commands.Products;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Products;

/// <summary>
/// Handler for updating product stock
/// </summary>
public class UpdateProductStockCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProductStockCommandHandler> logger)
    : IRequestHandler<UpdateProductStockCommand>
{
    public async Task Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating stock for product {ProductId} to {StockQuantity}", 
            request.ProductId, request.StockQuantity);

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
        }

        product.SetStock(request.StockQuantity, request.Reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated stock for product {ProductId} to {StockQuantity}", 
            request.ProductId, request.StockQuantity);
    }
}