using ECommerce.Application.Commands.Categories;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Categories;

/// <summary>
/// Handler for CreateCategoryCommand
/// </summary>
public class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateCategoryCommandHandler> logger) : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating category: {Name}", request.Name);

        Category category;

        if (request.ParentCategoryId.HasValue)
        {
            // Create child category
            var parentCategory = await categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory == null)
            {
                throw new NotFoundException("Parent category", request.ParentCategoryId.Value);
            }

            category = Category.CreateChild(request.Name, request.Description, request.ParentCategoryId.Value, parentCategory.Level);
        }
        else
        {
            // Create root category
            category = Category.CreateRoot(request.Name, request.Description);
        }

        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);
        return category.Id;
    }
}