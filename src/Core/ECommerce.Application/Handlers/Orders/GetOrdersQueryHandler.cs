using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Orders;

namespace ECommerce.Application.Handlers.Orders;

/// <summary>
/// Handler for GetOrdersQuery
/// </summary>
public class GetOrdersQueryHandler(
    IOrderQueryService orderQueryService,
    ILogger<GetOrdersQueryHandler> logger)
    : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetOrdersQuery with search term: {SearchTerm}, page: {Page}, pageSize: {PageSize}",
            request.SearchTerm, request.Page, request.PageSize);

        try
        {
            var result = await orderQueryService.GetOrdersAsync(request, cancellationToken);

            logger.LogInformation("Successfully retrieved {Count} orders out of {Total} total",
                result.Items.Count, result.TotalCount);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while handling GetOrdersQuery");
            throw;
        }
    }
}