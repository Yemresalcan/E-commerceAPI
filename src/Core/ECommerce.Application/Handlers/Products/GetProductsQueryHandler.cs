using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Products;

namespace ECommerce.Application.Handlers.Products;

/// <summary>
/// Handler for GetProductsQuery
/// </summary>
public class GetProductsQueryHandler(
    IProductQueryService productQueryService,
    ILogger<GetProductsQueryHandler> logger)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["SearchTerm"] = request.SearchTerm ?? "null",
            ["Page"] = request.Page,
            ["PageSize"] = request.PageSize,
            ["CategoryId"] = request.CategoryId?.ToString() ?? "null",
            ["MinPrice"] = request.MinPrice?.ToString() ?? "null",
            ["MaxPrice"] = request.MaxPrice?.ToString() ?? "null"
        }))
        {
            logger.LogInformation("Starting product query with search term: '{SearchTerm}', page: {Page}, pageSize: {PageSize}",
                request.SearchTerm ?? "null", request.Page, request.PageSize);

            try
            {
                var result = await productQueryService.GetProductsAsync(request, cancellationToken);

                logger.LogInformation("Successfully retrieved {Count} products out of {Total} total for query", 
                    result.Items.Count, result.TotalCount);

                // Log additional metrics for monitoring
                logger.LogDebug("Query performance metrics - Page: {Page}, PageSize: {PageSize}, TotalPages: {TotalPages}, HasNextPage: {HasNextPage}", 
                    result.Page, result.PageSize, result.TotalPages, result.HasNextPage);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while executing product query");
                throw;
            }
        }
    }
}