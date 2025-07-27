using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;
using ECommerce.ReadModel.Models;
using ECommerce.ReadModel.Services;

namespace ECommerce.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Event handler for OrderPlacedEvent
/// </summary>
public class OrderPlacedEventHandler : IEventHandler<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedEventHandler> _logger;
    private readonly IOrderSearchService _orderSearchService;

    public OrderPlacedEventHandler(
        ILogger<OrderPlacedEventHandler> logger,
        IOrderSearchService orderSearchService)
    {
        _logger = logger;
        _orderSearchService = orderSearchService;
    }

    /// <inheritdoc />
    public async Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling OrderPlacedEvent for order {OrderId} by customer {CustomerId}", 
            domainEvent.OrderId, domainEvent.CustomerId);

        try
        {
            // Create read model for Elasticsearch
            var orderReadModel = new OrderReadModel
            {
                Id = domainEvent.OrderId,
                CustomerId = domainEvent.CustomerId,
                Customer = new CustomerSummaryReadModel
                {
                    Id = domainEvent.CustomerId
                },
                Status = "Placed",
                ShippingAddress = domainEvent.ShippingAddress,
                BillingAddress = domainEvent.ShippingAddress, // Assuming same as shipping for now
                Items = [], // Will be populated by separate events or queries
                TotalAmount = domainEvent.TotalAmount,
                Currency = domainEvent.Currency,
                TotalItemCount = domainEvent.ItemCount,
                CreatedAt = domainEvent.OccurredOn,
                UpdatedAt = domainEvent.OccurredOn,
                ConfirmedAt = null,
                ShippedAt = null,
                DeliveredAt = null,
                CancelledAt = null
            };

            // Index the order in Elasticsearch
            var success = await _orderSearchService.IndexDocumentAsync(orderReadModel, cancellationToken);
            
            if (!success)
            {
                _logger.LogWarning("Failed to index order {OrderId} in Elasticsearch", domainEvent.OrderId);
            }
            
            _logger.LogInformation("Successfully processed OrderPlacedEvent for order {OrderId}", 
                domainEvent.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderPlacedEvent for order {OrderId}", 
                domainEvent.OrderId);
            throw;
        }
    }
}