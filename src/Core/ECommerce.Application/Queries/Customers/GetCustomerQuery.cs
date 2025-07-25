using ECommerce.Application.DTOs;

namespace ECommerce.Application.Queries.Customers;

/// <summary>
/// Query to get a single customer by ID
/// </summary>
public record GetCustomerQuery(Guid CustomerId) : IRequest<CustomerDto?>;