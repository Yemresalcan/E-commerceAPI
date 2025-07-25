using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Queries.Customers;

/// <summary>
/// Query to get customers with pagination and filtering
/// </summary>
public record GetCustomersQuery(
    string? SearchTerm = null,
    string? Email = null,
    string? PhoneNumber = null,
    bool? IsActive = null,
    string? Segment = null,
    string? Country = null,
    string? State = null,
    string? City = null,
    DateTime? RegistrationStartDate = null,
    DateTime? RegistrationEndDate = null,
    decimal? MinLifetimeValue = null,
    decimal? MaxLifetimeValue = null,
    int? MinOrders = null,
    int? MaxOrders = null,
    string? PreferredLanguage = null,
    string? SortBy = "registration_desc",
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CustomerDto>>;