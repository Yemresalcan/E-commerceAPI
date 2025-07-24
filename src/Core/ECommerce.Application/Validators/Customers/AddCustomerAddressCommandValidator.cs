using ECommerce.Application.Commands.Customers;
using ECommerce.Domain.Aggregates.CustomerAggregate;

namespace ECommerce.Application.Validators.Customers;

/// <summary>
/// Validator for AddCustomerAddressCommand
/// </summary>
public class AddCustomerAddressCommandValidator : AbstractValidator<AddCustomerAddressCommand>
{
    public AddCustomerAddressCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Address type must be a valid option");

        RuleFor(x => x.Street1)
            .NotEmpty()
            .WithMessage("Street address is required")
            .MaximumLength(100)
            .WithMessage("Street address cannot exceed 100 characters");

        RuleFor(x => x.Street2)
            .MaximumLength(100)
            .WithMessage("Street address line 2 cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Street2));

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(50)
            .WithMessage("City cannot exceed 50 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required")
            .MaximumLength(50)
            .WithMessage("State cannot exceed 50 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Postal code is required")
            .MaximumLength(20)
            .WithMessage("Postal code cannot exceed 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(50)
            .WithMessage("Country cannot exceed 50 characters");

        RuleFor(x => x.Label)
            .MaximumLength(50)
            .WithMessage("Address label cannot exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Label));
    }
}