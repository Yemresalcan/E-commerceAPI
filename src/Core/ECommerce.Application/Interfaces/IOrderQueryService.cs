using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Queries.Orders;

namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for order query operations
/// </summary>
public interface IOrderQueryService
{
    /// <summary>
    /// Gets a single order by ID
    /// </summary>
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders with pagination and filtering
    /// </summary>
    Task<PagedResult<OrderDto>> GetOrdersAsync(GetOrdersQuery query, CancellationToken cancellationToken = default);
}