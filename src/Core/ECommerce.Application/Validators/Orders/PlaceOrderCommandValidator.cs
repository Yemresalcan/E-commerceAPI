using ECommerce.Application.Commands.Orders;

namespace ECommerce.Application.Validators.Orders;

/// <summary>
/// Validator for PlaceOrderCommand
/// </summary>
public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty()
            .WithMessage("Shipping address is required")
            .MaximumLength(500)
            .WithMessage("Shipping address cannot exceed 500 characters");

        RuleFor(x => x.BillingAddress)
            .NotEmpty()
            .WithMessage("Billing address is required")
            .MaximumLength(500)
            .WithMessage("Billing address cannot exceed 500 characters");

        RuleFor(x => x.OrderItems)
            .NotEmpty()
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.OrderItems)
            .SetValidator(new OrderItemDtoValidator());
    }
}

/// <summary>
/// Validator for OrderItemDto
/// </summary>
public class OrderItemDtoValidator : AbstractValidator<ECommerce.Application.DTOs.OrderItemDto>
{
    public OrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(255)
            .WithMessage("Product name cannot exceed 255 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-character code (e.g., USD, EUR)");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount cannot be negative");
    }
}