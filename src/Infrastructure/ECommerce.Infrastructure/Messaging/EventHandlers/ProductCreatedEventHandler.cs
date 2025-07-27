using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;

namespace ECommerce.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Event handler for ProductCreatedEvent
/// </summary>
public class ProductCreatedEventHandler : IEventHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ProductCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling ProductCreatedEvent for product {ProductId} - {ProductName}", 
            domainEvent.ProductId, domainEvent.Name);

        try
        {
            // TODO: Update read model in Elasticsearch
            // TODO: Send notifications
            // TODO: Update analytics
            
            await Task.Delay(100, cancellationToken); // Simulate processing
            
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