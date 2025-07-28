using ECommerce.Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace ECommerce.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<IValidator<TestRequest>> _validatorMock;
    private readonly Mock<RequestHandlerDelegate<TestResponse>> _nextMock;
    private readonly ValidationBehavior<TestRequest, TestResponse> _behavior;

    public ValidationBehaviorTests()
    {
        _validatorMock = new Mock<IValidator<TestRequest>>();
        _nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        _behavior = new ValidationBehavior<TestRequest, TestResponse>(new[] { _validatorMock.Object });
    }

    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        // Arrange
        var behaviorWithoutValidators = new ValidationBehavior<TestRequest, TestResponse>(Enumerable.Empty<IValidator<TestRequest>>());
        var request = new TestRequest("Test");
        var expectedResponse = new TestResponse("Success");
        
        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behaviorWithoutValidators.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, result);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CallsNext()
    {
        // Arrange
        var request = new TestRequest("Test");
        var expectedResponse = new TestResponse("Success");
        var validationResult = new ValidationResult();

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        
        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, result);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var request = new TestRequest("Test");
        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Email", "Email is invalid")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _behavior.Handle(request, _nextMock.Object, CancellationToken.None));

        Assert.Equal(2, exception.Errors.Count());
        _nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ValidatesAll()
    {
        // Arrange
        var validator1Mock = new Mock<IValidator<TestRequest>>();
        var validator2Mock = new Mock<IValidator<TestRequest>>();
        var behaviorWithMultipleValidators = new ValidationBehavior<TestRequest, TestResponse>(
            new[] { validator1Mock.Object, validator2Mock.Object });

        var request = new TestRequest("Test");
        var expectedResponse = new TestResponse("Success");

        validator1Mock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        validator2Mock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behaviorWithMultipleValidators.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, result);
        validator1Mock.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        validator2Mock.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleValidatorsAndFailures_CombinesAllFailures()
    {
        // Arrange
        var validator1Mock = new Mock<IValidator<TestRequest>>();
        var validator2Mock = new Mock<IValidator<TestRequest>>();
        var behaviorWithMultipleValidators = new ValidationBehavior<TestRequest, TestResponse>(
            new[] { validator1Mock.Object, validator2Mock.Object });

        var request = new TestRequest("Test");

        validator1Mock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));
        
        validator2Mock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Email", "Email is invalid") }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => behaviorWithMultipleValidators.Handle(request, _nextMock.Object, CancellationToken.None));

        Assert.Equal(2, exception.Errors.Count());
        Assert.Contains(exception.Errors, e => e.PropertyName == "Name");
        Assert.Contains(exception.Errors, e => e.PropertyName == "Email");
        _nextMock.Verify(x => x(), Times.Never);
    }

    public record TestRequest(string Name) : IRequest<TestResponse>;
    public record TestResponse(string Message);
}