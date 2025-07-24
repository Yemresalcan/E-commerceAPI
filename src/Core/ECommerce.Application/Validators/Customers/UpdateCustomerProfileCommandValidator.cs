using ECommerce.Application.Commands.Customers;
using ECommerce.Domain.Aggregates.CustomerAggregate;

namespace ECommerce.Application.Validators.Customers;

/// <summary>
/// Validator for UpdateCustomerProfileCommand
/// </summary>
public class UpdateCustomerProfileCommandValidator : AbstractValidator<UpdateCustomerProfileCommand>
{
    public UpdateCustomerProfileCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.PreferredLanguage)
            .NotEmpty()
            .WithMessage("Preferred language is required")
            .Length(2)
            .WithMessage("Preferred language must be a 2-character ISO 639-1 code");

        RuleFor(x => x.PreferredCurrency)
            .IsInEnum()
            .WithMessage("Preferred currency must be a valid currency");

        RuleFor(x => x.Timezone)
            .NotEmpty()
            .WithMessage("Timezone is required")
            .MaximumLength(50)
            .WithMessage("Timezone cannot exceed 50 characters");

        RuleFor(x => x.CommunicationPreference)
            .IsInEnum()
            .WithMessage("Communication preference must be a valid option");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .WithMessage("Date of birth cannot be in the future")
            .GreaterThan(DateTime.Today.AddYears(-150))
            .WithMessage("Date of birth cannot be more than 150 years ago")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .MaximumLength(20)
            .WithMessage("Gender cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Gender));

        RuleFor(x => x.Interests)
            .MaximumLength(500)
            .WithMessage("Interests cannot exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Interests));
    }
}