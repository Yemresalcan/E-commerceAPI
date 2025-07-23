using ECommerce.Application.Queries;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Application.Tests.Queries;

public class TestQueryTests
{
    [Fact]
    public void TestQuery_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = 123;

        // Act
        var query = new TestQuery(id);

        // Assert
        query.Id.Should().Be(id);
    }
}

public class TestQueryHandlerTests
{
    private readonly Mock<ILogger<TestQueryHandler>> _loggerMock;
    private readonly TestQueryHandler _handler;

    public TestQueryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<TestQueryHandler>>();
        _handler = new TestQueryHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnQueryResult()
    {
        // Arrange
        var query = new TestQuery(123);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be("Query result for ID: 123");
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var query = new TestQuery(123);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing test query with ID: 123")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}