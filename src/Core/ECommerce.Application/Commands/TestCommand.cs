using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Commands;

/// <summary>
/// Test command to verify CQRS infrastructure is working
/// </summary>
/// <param name="Message">Test message</param>
public record TestCommand(string Message) : ICommand<string>;

/// <summary>
/// Validator for TestCommand
/// </summary>
public class TestCommandValidator : AbstractValidator<TestCommand>
{
    public TestCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required")
            .MaximumLength(100)
            .WithMessage("Message must not exceed 100 characters");
    }
}

/// <summary>
/// Handler for TestCommand
/// </summary>
public class TestCommandHandler(ILogger<TestCommandHandler> logger) : ICommandHandler<TestCommand, string>
{
    public Task<string> Handle(TestCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing test command with message: {Message}", request.Message);
        return Task.FromResult($"Processed: {request.Message}");
    }
}