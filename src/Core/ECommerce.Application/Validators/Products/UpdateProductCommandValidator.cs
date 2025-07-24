using ECommerce.Application.Commands.Products;

namespace ECommerce.Application.Validators.Products;

/// <summary>
/// Validator for UpdateProductCommand
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(255)
            .WithMessage("Product name cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Product description is required")
            .MaximumLength(2000)
            .WithMessage("Product description cannot exceed 2000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Product price must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-character code (e.g., USD, EUR)");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Product weight cannot be negative");

        RuleFor(x => x.Dimensions)
            .MaximumLength(100)
            .WithMessage("Dimensions cannot exceed 100 characters");
    }
}