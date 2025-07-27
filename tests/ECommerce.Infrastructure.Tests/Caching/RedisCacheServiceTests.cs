using ECommerce.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace ECommerce.Infrastructure.Tests.Caching;

public class RedisCacheServiceTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexerMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly Mock<IServer> _serverMock;
    private readonly Mock<ILogger<RedisCacheService>> _loggerMock;
    private readonly RedisCacheService _cacheService;

    public RedisCacheServiceTests()
    {
        _distributedCacheMock = new Mock<IDistributedCache>();
        _connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _serverMock = new Mock<IServer>();
        _loggerMock = new Mock<ILogger<RedisCacheService>>();

        _connectionMultiplexerMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

        _cacheService = new RedisCacheService(
            _distributedCacheMock.Object,
            _connectionMultiplexerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedValue()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestObject { Id = 1, Name = "Test" };
        var serializedValue = JsonSerializer.Serialize(testObject);

        _distributedCacheMock.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serializedValue);

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testObject.Id);
        result.Name.Should().Be(testObject.Name);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var key = "non-existent-key";

        _distributedCacheMock.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ShouldCallDistributedCacheWithOptions()
    {
        // Arrange
        var key = "test-key";
        var testObject = new TestObject { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMinutes(30);

        // Act
        await _cacheService.SetAsync(key, testObject, expiration);

        // Assert
        _distributedCacheMock.Verify(x => x.SetStringAsync(
            key,
            It.IsAny<string>(),
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == expiration),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallDistributedCacheRemove()
    {
        // Arrange
        var key = "test-key";

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _distributedCacheMock.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        // Arrange
        var key = "test-key";

        _databaseMock.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var key = "test-key";

        _databaseMock.Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_WhenKeyExists_ShouldReturnCachedValue()
    {
        // Arrange
        var key = "test-key";
        var cachedObject = new TestObject { Id = 1, Name = "Cached" };
        var serializedValue = JsonSerializer.Serialize(cachedObject);
        var factoryCalled = false;

        _distributedCacheMock.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serializedValue);

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () =>
        {
            factoryCalled = true;
            return Task.FromResult(new TestObject { Id = 2, Name = "Factory" });
        }, TimeSpan.FromMinutes(30));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(cachedObject.Id);
        result.Name.Should().Be(cachedObject.Name);
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_WhenKeyDoesNotExist_ShouldCallFactoryAndCache()
    {
        // Arrange
        var key = "test-key";
        var factoryObject = new TestObject { Id = 2, Name = "Factory" };
        var factoryCalled = false;

        _distributedCacheMock.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () =>
        {
            factoryCalled = true;
            return Task.FromResult(factoryObject);
        }, TimeSpan.FromMinutes(30));

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(factoryObject.Id);
        result.Name.Should().Be(factoryObject.Name);
        factoryCalled.Should().BeTrue();

        _distributedCacheMock.Verify(x => x.SetStringAsync(
            key,
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}