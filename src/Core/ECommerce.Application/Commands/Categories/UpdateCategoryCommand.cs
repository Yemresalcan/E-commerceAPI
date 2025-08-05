namespace ECommerce.Application.Commands.Categories;

/// <summary>
/// Command to update an existing category
/// </summary>
public record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string Description
) : IRequest;