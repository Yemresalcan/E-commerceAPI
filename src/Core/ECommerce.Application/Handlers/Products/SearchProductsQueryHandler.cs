using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Products;

namespace ECommerce.Application.Handlers.Products;

/// <summary>
/// Handler for SearchProductsQuery
/// </summary>
public class SearchProductsQueryHandler(
    IProductQueryService productQueryService,
    ILogger<SearchProductsQueryHandler> logger)
    : IRequestHandler<SearchProductsQuery, ProductSearchResultDto>
{
    public async Task<ProductSearchResultDto> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling SearchProductsQuery with query: {Query}, page: {Page}, pageSize: {PageSize}",
            request.Query, request.Page, request.PageSize);

        try
        {
            var result = await productQueryService.SearchProductsAsync(request, cancellationToken);

            logger.LogInformation("Successfully searched products: {Count} results out of {Total} total",
                result.Products.Count(), result.TotalCount);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while handling SearchProductsQuery");
            throw;
        }
    }
}