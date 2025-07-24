using ECommerce.Application.Commands.Customers;
using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Customers;

/// <summary>
/// Handler for AddCustomerAddressCommand
/// </summary>
public class AddCustomerAddressCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<AddCustomerAddressCommandHandler> logger
) : IRequestHandler<AddCustomerAddressCommand, Guid>
{
    public async Task<Guid> Handle(AddCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding address for customer: {CustomerId}", request.CustomerId);

        // Get customer
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID '{request.CustomerId}' not found");
        }

        // Create address
        var address = Address.Create(
            request.Type,
            request.Street1,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.Street2,
            request.Label,
            request.IsPrimary
        );

        // Add address to customer
        customer.AddAddress(address);

        // Update repository
        customerRepository.Update(customer);

        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully added address with ID: {AddressId} for customer: {CustomerId}", 
            address.Id, request.CustomerId);

        return address.Id;
    }
}