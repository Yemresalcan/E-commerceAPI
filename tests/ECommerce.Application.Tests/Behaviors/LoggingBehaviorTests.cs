using ECommerce.Application.Behaviors;
using ECommerce.Application.Commands;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Application.Tests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestCommand, string>>> _loggerMock;
    private readonly LoggingBehavior<TestCommand, string> _behavior;

    public LoggingBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<LoggingBehavior<TestCommand, string>>>();
        _behavior = new LoggingBehavior<TestCommand, string>(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithSuccessfulRequest_ShouldLogStartAndEnd()
    {
        // Arrange
        var command = new TestCommand("Test message");
        var expectedResponse = "Success";

        Task<string> Next() => Task.FromResult(expectedResponse);

        // Act
        var result = await _behavior.Handle(command, Next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling TestCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handled TestCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithException_ShouldLogError()
    {
        // Arrange
        var command = new TestCommand("Test message");
        var exception = new InvalidOperationException("Test exception");

        Task<string> Next() => throw exception;

        // Act & Assert
        await _behavior.Invoking(b => b.Handle(command, Next, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling TestCommand")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}