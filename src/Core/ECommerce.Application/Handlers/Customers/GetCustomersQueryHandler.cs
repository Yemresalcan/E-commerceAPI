using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Customers;

namespace ECommerce.Application.Handlers.Customers;

/// <summary>
/// Handler for GetCustomersQuery
/// </summary>
public class GetCustomersQueryHandler(
    ICustomerQueryService customerQueryService,
    ILogger<GetCustomersQueryHandler> logger)
    : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetCustomersQuery with search term: {SearchTerm}, page: {Page}, pageSize: {PageSize}",
            request.SearchTerm, request.Page, request.PageSize);

        try
        {
            var result = await customerQueryService.GetCustomersAsync(request, cancellationToken);

            logger.LogInformation("Successfully retrieved {Count} customers out of {Total} total",
                result.Items.Count, result.TotalCount);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while handling GetCustomersQuery");
            throw;
        }
    }
}