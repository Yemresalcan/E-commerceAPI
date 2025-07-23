using ECommerce.Application.Behaviors;
using ECommerce.Application.Commands;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<IValidator<TestCommand>> _validatorMock;
    private readonly ValidationBehavior<TestCommand, string> _behavior;

    public ValidationBehaviorTests()
    {
        _validatorMock = new Mock<IValidator<TestCommand>>();
        _behavior = new ValidationBehavior<TestCommand, string>([_validatorMock.Object]);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var command = new TestCommand("Valid message");
        var expectedResponse = "Success";
        var nextCalled = false;

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Task<string> Next() 
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }

        // Act
        var result = await _behavior.Handle(command, Next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var command = new TestCommand("");
        var validationFailures = new List<ValidationFailure>
        {
            new("Message", "Message is required")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        Task<string> Next() => Task.FromResult("Should not be called");

        // Act & Assert
        await _behavior.Invoking(b => b.Handle(command, Next, CancellationToken.None))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var command = new TestCommand("Test message");
        var expectedResponse = "Success";
        var nextCalled = false;
        var behaviorWithNoValidators = new ValidationBehavior<TestCommand, string>([]);

        Task<string> Next() 
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }

        // Act
        var result = await behaviorWithNoValidators.Handle(command, Next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
    }
}