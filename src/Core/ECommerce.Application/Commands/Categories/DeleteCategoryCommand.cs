namespace ECommerce.Application.Commands.Categories;

/// <summary>
/// Command to delete a category
/// </summary>
public record DeleteCategoryCommand(
    Guid CategoryId
) : IRequest;