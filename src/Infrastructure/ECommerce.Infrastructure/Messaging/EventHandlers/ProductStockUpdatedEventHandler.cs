using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;
using ECommerce.Domain.Interfaces;
using ECommerce.ReadModel.Models;
using ECommerce.ReadModel.Services;

namespace ECommerce.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Event handler for ProductStockUpdatedEvent - syncs stock changes to Elasticsearch
/// </summary>
public class ProductStockUpdatedEventHandler : IEventHandler<ProductStockUpdatedEvent>
{
    private readonly ILogger<ProductStockUpdatedEventHandler> _logger;
    private readonly IProductRepository _productRepository;
    private readonly IProductSearchService _productSearchService;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public ProductStockUpdatedEventHandler(
        ILogger<ProductStockUpdatedEventHandler> logger,
        IProductRepository productRepository,
        IProductSearchService productSearchService,
        ICacheInvalidationService cacheInvalidationService)
    {
        _logger = logger;
        _productRepository = productRepository;
        _productSearchService = productSearchService;
        _cacheInvalidationService = cacheInvalidationService;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ProductStockUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling ProductStockUpdatedEvent for product {ProductId} - Stock changed from {PreviousStock} to {NewStock}. Reason: {Reason}", 
            domainEvent.ProductId, domainEvent.PreviousStock, domainEvent.NewStock, domainEvent.Reason);

        try
        {
            // Get the updated product from database
            var product = await _productRepository.GetByIdAsync(domainEvent.ProductId, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found in database during stock update sync", domainEvent.ProductId);
                return;
            }

            // Create updated read model for Elasticsearch
            var productReadModel = new ProductReadModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Sku = product.Sku,
                Price = product.Price.Amount,
                Currency = product.Price.Currency,
                StockQuantity = product.StockQuantity, // This should be the updated stock
                MinimumStockLevel = product.MinimumStockLevel,
                Category = new CategoryReadModel
                {
                    Id = product.CategoryId,
                    Name = "Default Category", // TODO: Get actual category name
                    Description = "",
                    ParentCategoryId = null,
                    CategoryPath = ""
                },
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                Weight = product.Weight,
                Dimensions = product.Dimensions,
                AverageRating = product.AverageRating,
                ReviewCount = product.ReviewCount,
                IsInStock = product.IsInStock,
                IsLowStock = product.IsLowStock,
                IsOutOfStock = product.IsOutOfStock,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Tags = new List<string>(),
                Suggest = new Nest.CompletionField
                {
                    Input = [product.Name]
                }
            };

            // Update the product in Elasticsearch
            var success = await _productSearchService.IndexDocumentAsync(productReadModel, cancellationToken);
            
            if (success)
            {
                _logger.LogInformation("Successfully synced stock update to Elasticsearch for product {ProductId}. New stock: {NewStock}", 
                    domainEvent.ProductId, domainEvent.NewStock);
            }
            else
            {
                _logger.LogWarning("Failed to sync stock update to Elasticsearch for product {ProductId}", domainEvent.ProductId);
            }

            // Invalidate product cache
            await _cacheInvalidationService.InvalidateProductCacheAsync(
                domainEvent.ProductId, 
                product.CategoryId, 
                cancellationToken);
            
            _logger.LogInformation("Successfully processed ProductStockUpdatedEvent for product {ProductId}", 
                domainEvent.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductStockUpdatedEvent for product {ProductId}", 
                domainEvent.ProductId);
            throw;
        }
    }
}