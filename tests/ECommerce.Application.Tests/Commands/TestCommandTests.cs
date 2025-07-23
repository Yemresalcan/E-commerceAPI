using ECommerce.Application.Commands;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Application.Tests.Commands;

public class TestCommandTests
{
    [Fact]
    public void TestCommand_ShouldHaveCorrectProperties()
    {
        // Arrange
        var message = "Test message";

        // Act
        var command = new TestCommand(message);

        // Assert
        command.Message.Should().Be(message);
    }
}

public class TestCommandValidatorTests
{
    private readonly TestCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidMessage_ShouldPass()
    {
        // Arrange
        var command = new TestCommand("Valid message");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyMessage_ShouldFail(string? message)
    {
        // Arrange
        var command = new TestCommand(message!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TestCommand.Message));
    }

    [Fact]
    public void Validate_WithTooLongMessage_ShouldFail()
    {
        // Arrange
        var longMessage = new string('a', 101);
        var command = new TestCommand(longMessage);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(TestCommand.Message));
    }
}

public class TestCommandHandlerTests
{
    private readonly Mock<ILogger<TestCommandHandler>> _loggerMock;
    private readonly TestCommandHandler _handler;

    public TestCommandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<TestCommandHandler>>();
        _handler = new TestCommandHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProcessedMessage()
    {
        // Arrange
        var command = new TestCommand("Test message");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Processed: Test message");
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var command = new TestCommand("Test message");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing test command with message: Test message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}