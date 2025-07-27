using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;
using ECommerce.ReadModel.Models;
using ECommerce.ReadModel.Services;

namespace ECommerce.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Event handler for CustomerRegisteredEvent
/// </summary>
public class CustomerRegisteredEventHandler : IEventHandler<CustomerRegisteredEvent>
{
    private readonly ILogger<CustomerRegisteredEventHandler> _logger;
    private readonly ICustomerSearchService _customerSearchService;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public CustomerRegisteredEventHandler(
        ILogger<CustomerRegisteredEventHandler> logger,
        ICustomerSearchService customerSearchService,
        ICacheInvalidationService cacheInvalidationService)
    {
        _logger = logger;
        _customerSearchService = customerSearchService;
        _cacheInvalidationService = cacheInvalidationService;
    }

    /// <inheritdoc />
    public async Task HandleAsync(CustomerRegisteredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CustomerRegisteredEvent for customer {CustomerId} - {Email}", 
            domainEvent.CustomerId, domainEvent.Email);

        try
        {
            // Create read model for Elasticsearch
            var customerReadModel = new CustomerReadModel
            {
                Id = domainEvent.CustomerId,
                FirstName = domainEvent.FirstName,
                LastName = domainEvent.LastName,
                FullName = $"{domainEvent.FirstName} {domainEvent.LastName}".Trim(),
                Email = domainEvent.Email,
                PhoneNumber = domainEvent.PhoneNumber,
                IsActive = true,
                RegistrationDate = domainEvent.RegistrationDate,
                LastActiveDate = domainEvent.RegistrationDate,
                Addresses = [],
                Profile = new ProfileReadModel
                {
                    PreferredLanguage = "en",
                    PreferredCurrency = "USD",
                    MarketingEmailsEnabled = true,
                    SmsNotificationsEnabled = false,
                    Interests = []
                },
                Statistics = new CustomerStatisticsReadModel
                {
                    TotalOrders = 0,
                    TotalSpent = 0,
                    Currency = "USD",
                    AverageOrderValue = 0,
                    LifetimeValue = 0,
                    Segment = "New"
                },
                CreatedAt = domainEvent.OccurredOn,
                UpdatedAt = domainEvent.OccurredOn,
                Suggest = new Nest.CompletionField
                {
                    Input = [domainEvent.Email, $"{domainEvent.FirstName} {domainEvent.LastName}".Trim()]
                }
            };

            // Index the customer in Elasticsearch
            var success = await _customerSearchService.IndexDocumentAsync(customerReadModel, cancellationToken);
            
            if (!success)
            {
                _logger.LogWarning("Failed to index customer {CustomerId} in Elasticsearch", domainEvent.CustomerId);
            }

            // Invalidate customer cache
            await _cacheInvalidationService.InvalidateCustomerCacheAsync(
                domainEvent.CustomerId, 
                cancellationToken);
            
            _logger.LogInformation("Successfully processed CustomerRegisteredEvent for customer {CustomerId}", 
                domainEvent.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling CustomerRegisteredEvent for customer {CustomerId}", 
                domainEvent.CustomerId);
            throw;
        }
    }
}