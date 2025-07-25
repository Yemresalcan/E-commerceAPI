using ECommerce.Application.DTOs;

namespace ECommerce.Application.Queries.Orders;

/// <summary>
/// Query to get a single order by ID
/// </summary>
public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto?>;