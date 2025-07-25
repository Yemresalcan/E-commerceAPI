using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Queries.Orders;

/// <summary>
/// Query to get orders with pagination and filtering
/// </summary>
public record GetOrdersQuery(
    string? SearchTerm = null,
    Guid? CustomerId = null,
    string? Status = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? PaymentMethod = null,
    string? PaymentStatus = null,
    string? SortBy = "created_desc",
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderDto>>;