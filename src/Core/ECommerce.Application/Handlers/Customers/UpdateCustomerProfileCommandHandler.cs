using ECommerce.Application.Commands.Customers;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Customers;

/// <summary>
/// Handler for UpdateCustomerProfileCommand
/// </summary>
public class UpdateCustomerProfileCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateCustomerProfileCommandHandler> logger
) : IRequestHandler<UpdateCustomerProfileCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCustomerProfileCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating profile for customer: {CustomerId}", request.CustomerId);

        // Get customer
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID '{request.CustomerId}' not found");
        }

        // Update localization preferences
        customer.Profile.UpdateLocalizationPreferences(
            request.PreferredLanguage,
            request.PreferredCurrency,
            request.Timezone
        );

        // Update communication preferences
        customer.Profile.UpdateCommunicationPreferences(
            request.CommunicationPreference,
            request.ReceiveMarketingEmails,
            request.ReceiveOrderNotifications,
            request.ReceivePromotionalSms
        );

        // Update personal information if provided
        if (request.DateOfBirth.HasValue || !string.IsNullOrWhiteSpace(request.Gender) || !string.IsNullOrWhiteSpace(request.Interests))
        {
            customer.Profile.UpdatePersonalInfo(
                request.DateOfBirth,
                request.Gender,
                request.Interests
            );
        }

        // Update repository
        customerRepository.Update(customer);

        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated profile for customer: {CustomerId}", request.CustomerId);

        return Unit.Value;
    }
}