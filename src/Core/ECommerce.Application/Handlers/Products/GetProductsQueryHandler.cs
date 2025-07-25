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
        logger.LogInformation("Handling GetProductsQuery with search term: {SearchTerm}, page: {Page}, pageSize: {PageSize}",
            request.SearchTerm, request.Page, request.PageSize);

        try
        {
            var result = await productQueryService.GetProductsAsync(request, cancellationToken);

            logger.LogInformation("Successfully retrieved {Count} products out of {Total} total",
                result.Items.Count, result.TotalCount);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while handling GetProductsQuery");
            throw;
        }
    }
}