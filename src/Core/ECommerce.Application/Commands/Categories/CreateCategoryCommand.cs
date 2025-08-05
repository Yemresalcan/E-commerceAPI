using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Commands.Categories;

/// <summary>
/// Command to create a new category
/// </summary>
public record CreateCategoryCommand(
    string Name,
    string Description,
    Guid? ParentCategoryId = null
) : ICommand<Guid>;