using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;
using ECommerce.ReadModel.Models;
using ECommerce.ReadModel.Services;

namespace ECommerce.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Event handler for ProductCreatedEvent
/// </summary>
public class ProductCreatedEventHandler : IEventHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;
    private readonly IProductSearchService _productSearchService;

    public ProductCreatedEventHandler(
        ILogger<ProductCreatedEventHandler> logger,
        IProductSearchService productSearchService)
    {
        _logger = logger;
        _productSearchService = productSearchService;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ProductCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling ProductCreatedEvent for product {ProductId} - {ProductName}", 
            domainEvent.ProductId, domainEvent.Name);

        try
        {
            // Create read model for Elasticsearch
            var productReadModel = new ProductReadModel
            {
                Id = domainEvent.ProductId,
                Name = domainEvent.Name,
                Price = domainEvent.PriceAmount,
                Currency = domainEvent.Currency,
                StockQuantity = domainEvent.StockQuantity,
                Category = new CategoryReadModel
                {
                    Id = domainEvent.CategoryId
                },
                IsActive = true,
                IsFeatured = false,
                IsInStock = domainEvent.StockQuantity > 0,
                IsLowStock = domainEvent.StockQuantity <= 10,
                IsOutOfStock = domainEvent.StockQuantity == 0,
                CreatedAt = domainEvent.OccurredOn,
                UpdatedAt = domainEvent.OccurredOn,
                Tags = [],
                Suggest = new Nest.CompletionField
                {
                    Input = [domainEvent.Name]
                }
            };

            // Index the product in Elasticsearch
            var success = await _productSearchService.IndexDocumentAsync(productReadModel, cancellationToken);
            
            if (!success)
            {
                _logger.LogWarning("Failed to index product {ProductId} in Elasticsearch", domainEvent.ProductId);
            }
            
            _logger.LogInformation("Successfully processed ProductCreatedEvent for product {ProductId}", 
                domainEvent.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductCreatedEvent for product {ProductId}", 
                domainEvent.ProductId);
            throw;
        }
    }
}