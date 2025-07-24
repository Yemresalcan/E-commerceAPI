using ECommerce.Domain.Aggregates.CustomerAggregate;

namespace ECommerce.Application.Commands.Customers;

/// <summary>
/// Command to update customer profile information
/// </summary>
public record UpdateCustomerProfileCommand(
    Guid CustomerId,
    string PreferredLanguage,
    PreferredCurrency PreferredCurrency,
    string Timezone,
    CommunicationPreference CommunicationPreference,
    bool ReceiveMarketingEmails,
    bool ReceiveOrderNotifications,
    bool ReceivePromotionalSms,
    DateTime? DateOfBirth = null,
    string? Gender = null,
    string? Interests = null
) : IRequest<Unit>;