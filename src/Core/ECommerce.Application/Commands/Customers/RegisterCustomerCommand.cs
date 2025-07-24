namespace ECommerce.Application.Commands.Customers;

/// <summary>
/// Command to register a new customer
/// </summary>
public record RegisterCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber = null
) : IRequest<Guid>;