using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Caching;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Infrastructure.Tests.Caching;

public class CacheInvalidationServiceTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<CacheInvalidationService>> _loggerMock;
    private readonly CacheInvalidationService _invalidationService;

    public CacheInvalidationServiceTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<CacheInvalidationService>>();
        _invalidationService = new CacheInvalidationService(_cacheServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvalidateProductCacheAsync_WithProductId_ShouldRemoveSpecificProductAndPattern()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        // Act
        await _invalidationService.InvalidateProductCacheAsync(productId, categoryId);

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveAsync(
            $"product:{productId}", 
            It.IsAny<CancellationToken>()), Times.Once);

        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync(
            "products:*", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateProductCacheAsync_WithoutProductId_ShouldOnlyRemovePattern()
    {
        // Act
        await _invalidationService.InvalidateProductCacheAsync();

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveAsync(
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Never);

        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync(
            "products:*", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateOrderCacheAsync_WithOrderId_ShouldRemoveSpecificOrderAndPattern()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Act
        await _invalidationService.InvalidateOrderCacheAsync(orderId, customerId);

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveAsync(
            $"order:{orderId}", 
            It.IsAny<CancellationToken>()), Times.Once);

        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync(
            $"orders:*:{customerId}:*", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateOrderCacheAsync_WithoutOrderId_ShouldOnlyRemovePattern()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        await _invalidationService.InvalidateOrderCacheAsync(customerId: customerId);

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveAsync(
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Never);

        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync(
            $"orders:*:{customerId}:*", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateCustomerCacheAsync_WithCustomerId_ShouldRemoveSpecificCustomerAndPattern()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        await _invalidationService.InvalidateCustomerCacheAsync(customerId);

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveAsync(
            $"customer:{customerId}", 
            It.IsAny<CancellationToken>()), Times.Once);

        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync(
            "customers:*", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateCustomerCacheAsync_WithoutCustomerId_ShouldOnlyRemovePattern()
    {
        // Act
        await _invalidationService.InvalidateCustomerCacheAsync();

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveAsync(
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Never);

        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync(
            "customers:*", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateAllCacheAsync_ShouldRemoveAllEntries()
    {
        // Act
        await _invalidationService.InvalidateAllCacheAsync();

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync(
            "*", 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}