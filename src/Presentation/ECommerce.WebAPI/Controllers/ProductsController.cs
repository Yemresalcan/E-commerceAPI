using ECommerce.Application.Commands.Products;
using ECommerce.Application.Queries.Products;
using ECommerce.Application.DTOs;
using ECommerce.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ECommerce.Domain.Interfaces;
using ECommerce.ReadModel.Services;
using ECommerce.ReadModel.Models;

namespace ECommerce.WebAPI.Controllers;

/// <summary>
/// Products API controller providing CRUD operations for product management
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ProductsController(
    IMediator mediator,
    IProductRepository productRepository,
    IProductSearchService productSearchService) : ControllerBase
{
    /// <summary>
    /// Get products with pagination and filtering
    /// </summary>
    /// <param name="searchTerm">Search term for product name or description</param>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="inStockOnly">Filter to show only products in stock</param>
    /// <param name="featuredOnly">Filter to show only featured products</param>
    /// <param name="tags">Filter by product tags</param>
    /// <param name="minRating">Minimum rating filter</param>
    /// <param name="sortBy">Sort criteria (relevance, price_asc, price_desc, rating, newest)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? inStockOnly = null,
        [FromQuery] bool? featuredOnly = null,
        [FromQuery] List<string>? tags = null,
        [FromQuery] decimal? minRating = null,
        [FromQuery] string? sortBy = "relevance",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        var query = new GetProductsQuery(
            searchTerm,
            categoryId,
            minPrice,
            maxPrice,
            inStockOnly,
            featuredOnly,
            tags,
            minRating,
            sortBy,
            page,
            pageSize);

        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Search products using advanced search capabilities
    /// </summary>
    /// <param name="query">Search query string</param>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="inStockOnly">Filter to show only products in stock</param>
    /// <param name="featuredOnly">Filter to show only featured products</param>
    /// <param name="tags">Filter by product tags</param>
    /// <param name="minRating">Minimum rating filter</param>
    /// <param name="sortBy">Sort criteria</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with facets</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ProductSearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductSearchResultDto>> SearchProducts(
        [FromQuery] string query,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? inStockOnly = null,
        [FromQuery] bool? featuredOnly = null,
        [FromQuery] List<string>? tags = null,
        [FromQuery] decimal? minRating = null,
        [FromQuery] string? sortBy = "relevance",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query is required");

        if (page < 1)
            return BadRequest("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        var searchQuery = new SearchProductsQuery(
            query,
            categoryId,
            minPrice,
            maxPrice,
            inStockOnly,
            featuredOnly,
            tags,
            minRating,
            sortBy,
            page,
            pageSize);

        var result = await mediator.Send(searchQuery, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="command">Product creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<Guid>> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var productId = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProducts), new { }, productId);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Product update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.ProductId)
            return BadRequest("Product ID in URL does not match the ID in the request body");

        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Update product stock quantity
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="stockQuantity">New stock quantity</param>
    /// <param name="reason">Reason for stock update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductStock(
        Guid id,
        [FromQuery] int stockQuantity,
        [FromQuery] string reason = "Manual stock update",
        CancellationToken cancellationToken = default)
    {
        if (stockQuantity < 0)
            return BadRequest("Stock quantity cannot be negative");

        var command = new UpdateProductStockCommand(id, stockQuantity, reason);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sync products from database to Elasticsearch
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result</returns>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SyncProducts(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all products from database
            var products = await productRepository.GetActiveProductsAsync(cancellationToken);
            
            var syncedCount = 0;
            foreach (var product in products)
            {
                // Map domain entity to read model
                var productReadModel = new ECommerce.ReadModel.Models.ProductReadModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Sku = product.Sku,
                    Price = product.Price.Amount,
                    Currency = product.Price.Currency,
                    StockQuantity = product.StockQuantity,
                    MinimumStockLevel = product.MinimumStockLevel,
                    IsActive = product.IsActive,
                    IsFeatured = product.IsFeatured,
                    Weight = product.Weight,
                    Dimensions = product.Dimensions,
                    AverageRating = product.AverageRating,
                    ReviewCount = product.ReviewCount,
                    IsInStock = product.IsInStock,
                    IsLowStock = product.IsLowStock,
                    IsOutOfStock = product.IsOutOfStock,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    Tags = new List<string>(),
                    Category = new ECommerce.ReadModel.Models.CategoryReadModel
                    {
                        Id = product.CategoryId,
                        Name = "Default Category", // We'll need to get this from category service
                        Description = "",
                        ParentCategoryId = null,
                        CategoryPath = ""
                    }
                };

                // Index the product in Elasticsearch
                var indexed = await productSearchService.IndexDocumentAsync(productReadModel, cancellationToken);
                if (indexed)
                {
                    syncedCount++;
                }
            }

            // Refresh the index to make documents searchable immediately
            await productSearchService.RefreshIndexAsync(cancellationToken);

            return Ok(new { 
                Message = "Products synced successfully", 
                SyncedCount = syncedCount,
                TotalProducts = products.Count()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Message = "Error occurred while syncing products", 
                Error = ex.Message 
            });
        }
    }
}