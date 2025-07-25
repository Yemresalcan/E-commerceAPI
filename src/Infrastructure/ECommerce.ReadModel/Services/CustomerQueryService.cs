using AutoMapper;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Customers;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Implementation of customer query service using Elasticsearch
/// </summary>
public class CustomerQueryService(
    ICustomerSearchService customerSearchService,
    IMapper mapper,
    ILogger<CustomerQueryService> logger)
    : ICustomerQueryService
{
    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting customer by ID: {CustomerId}", customerId);

        var customer = await customerSearchService.GetDocumentAsync(customerId, cancellationToken);
        return customer == null ? null : mapper.Map<CustomerDto>(customer);
    }

    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(GetCustomersQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting customers with search term: {SearchTerm}", query.SearchTerm);

        var searchRequest = new CustomerSearchRequest
        {
            Query = query.SearchTerm,
            Email = query.Email,
            PhoneNumber = query.PhoneNumber,
            IsActive = query.IsActive,
            Segment = query.Segment,
            Country = query.Country,
            State = query.State,
            City = query.City,
            RegistrationStartDate = query.RegistrationStartDate,
            RegistrationEndDate = query.RegistrationEndDate,
            MinLifetimeValue = query.MinLifetimeValue,
            MaxLifetimeValue = query.MaxLifetimeValue,
            MinOrders = query.MinOrders,
            MaxOrders = query.MaxOrders,
            PreferredLanguage = query.PreferredLanguage,
            SortBy = query.SortBy,
            Page = query.Page,
            PageSize = query.PageSize
        };

        var searchResult = await customerSearchService.SearchCustomersAsync(searchRequest, cancellationToken);
        var customerDtos = mapper.Map<IEnumerable<CustomerDto>>(searchResult.Customers);

        return new PagedResult<CustomerDto>
        {
            Items = customerDtos.ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = (int)searchResult.TotalCount
        };
    }
}