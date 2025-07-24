using ECommerce.Domain.Aggregates.CustomerAggregate;

namespace ECommerce.Application.Commands.Customers;

/// <summary>
/// Command to add a new address to a customer
/// </summary>
public record AddCustomerAddressCommand(
    Guid CustomerId,
    AddressType Type,
    string Street1,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? Street2 = null,
    string? Label = null,
    bool IsPrimary = false
) : IRequest<Guid>;