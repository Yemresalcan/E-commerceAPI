using ECommerce.Application.Commands.Customers;
using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.Handlers.Customers;

/// <summary>
/// Handler for RegisterCustomerCommand
/// </summary>
public class RegisterCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<RegisterCustomerCommandHandler> logger
) : IRequestHandler<RegisterCustomerCommand, Guid>
{
    public async Task<Guid> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Registering new customer with email: {Email}", request.Email);

        // Create value objects
        var email = new Email(request.Email);
        
        // Check if customer with this email already exists
        var existingCustomer = await customerRepository.GetByEmailAsync(email, cancellationToken);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException($"Customer with email '{request.Email}' already exists");
        }
        PhoneNumber? phoneNumber = null;
        
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            phoneNumber = new PhoneNumber(request.PhoneNumber);
        }

        // Create customer
        var customer = Customer.Create(
            request.FirstName,
            request.LastName,
            email,
            phoneNumber
        );

        // Add to repository
        await customerRepository.AddAsync(customer, cancellationToken);

        // Save changes
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully registered customer with ID: {CustomerId}", customer.Id);

        return customer.Id;
    }
}