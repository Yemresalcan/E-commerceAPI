using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Queries.Customers;

namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for customer query operations
/// </summary>
public interface ICustomerQueryService
{
    /// <summary>
    /// Gets a single customer by ID
    /// </summary>
    Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers with pagination and filtering
    /// </summary>
    Task<PagedResult<CustomerDto>> GetCustomersAsync(GetCustomersQuery query, CancellationToken cancellationToken = default);
}