using ECommerce.Application.Commands.Categories;
using ECommerce.Application.Queries.Categories;
using ECommerce.Application.DTOs;
using ECommerce.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace ECommerce.WebAPI.Controllers;

/// <summary>
/// Categories API controller providing category management operations
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all categories with hierarchical structure
    /// </summary>
    /// <param name="includeInactive">Include inactive categories</param>
    /// <param name="parentId">Filter by parent category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(
        [FromQuery] bool includeInactive = false,
        [FromQuery] Guid? parentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery(includeInactive, parentId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Category with ID {id} was not found");

        return Ok(result);
    }

    /// <summary>
    /// Create a new root category
    /// </summary>
    /// <param name="command">Category creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created category ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<Guid>> CreateCategory(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var categoryId = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategory), new { id = categoryId }, categoryId);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="command">Category update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.CategoryId)
            return BadRequest("Category ID in URL does not match the ID in the request body");

        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}