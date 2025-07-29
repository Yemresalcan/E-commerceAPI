using AutoMapper;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Orders;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Implementation of order query service using Elasticsearch
/// </summary>
public class OrderQueryService(
    IOrderSearchService orderSearchService,
    IMapper mapper,
    ILogger<OrderQueryService> logger)
    : IOrderQueryService
{
    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting order by ID: {OrderId}", orderId);

        var order = await orderSearchService.GetDocumentAsync(orderId, cancellationToken);
        return order == null ? null : mapper.Map<OrderDto>(order);
    }

    public async Task<PagedResult<OrderDto>> GetOrdersAsync(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting orders with search term: {SearchTerm}", query.SearchTerm);

        var searchRequest = new OrderSearchRequest
        {
            Query = query.SearchTerm,
            CustomerId = query.CustomerId,
            Status = query.Status,
            MinAmount = query.MinAmount,
            MaxAmount = query.MaxAmount,
            StartDate = query.StartDate,
            EndDate = query.EndDate,
            PaymentMethod = query.PaymentMethod,
            PaymentStatus = query.PaymentStatus,
            SortBy = query.SortBy,
            Page = query.Page,
            PageSize = query.PageSize
        };

        var searchResult = await orderSearchService.SearchOrdersAsync(searchRequest, cancellationToken);
        var orderDtos = mapper.Map<IEnumerable<OrderDto>>(searchResult.Orders);

        return new PagedResult<OrderDto>
        {
            Items = orderDtos.ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = (int)searchResult.TotalCount
        };
    }

    public async Task CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Checking order query service health");
        
        // Test basic search functionality
        var searchRequest = new OrderSearchRequest
        {
            Query = "",
            Page = 1,
            PageSize = 1
        };
        
        await orderSearchService.SearchOrdersAsync(searchRequest, cancellationToken);
        logger.LogDebug("Order query service health check passed");
    }
}