using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers.Products;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Products;

namespace ECommerce.Application.Tests.Handlers.Products;

public class SearchProductsQueryHandlerTests
{
    private readonly Mock<IProductQueryService> _productQueryServiceMock;
    private readonly Mock<ILogger<SearchProductsQueryHandler>> _loggerMock;
    private readonly SearchProductsQueryHandler _handler;

    public SearchProductsQueryHandlerTests()
    {
        _productQueryServiceMock = new Mock<IProductQueryService>();
        _loggerMock = new Mock<ILogger<SearchProductsQueryHandler>>();
        _handler = new SearchProductsQueryHandler(
            _productQueryServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSearchResult_WhenProductsFound()
    {
        // Arrange
        var query = new SearchProductsQuery("laptop", Page: 1, PageSize: 10);

        var productDtos = new List<ProductDto>
        {
            new(Guid.NewGuid(), "Gaming Laptop", "", "", 1500, "USD", 5, 
                new CategoryDto(Guid.NewGuid(), "Electronics", "", null, ""), 
                true, true, 2.5m, "15x10x1", 4.5m, 120, true, false, DateTime.UtcNow, DateTime.UtcNow, ["gaming"]),
            new(Guid.NewGuid(), "Business Laptop", "", "", 800, "USD", 10, 
                new CategoryDto(Guid.NewGuid(), "Electronics", "", null, ""), 
                true, false, 2.0m, "14x9x1", 4.0m, 85, true, false, DateTime.UtcNow, DateTime.UtcNow, ["business"])
        };

        var facetsDto = new ProductSearchFacetsDto(
            new Dictionary<string, long> { { "Electronics", 2 } },
            new Dictionary<string, long> { { "500-1000", 1 }, { "1000-2000", 1 } },
            new Dictionary<string, long> { { "Dell", 1 }, { "HP", 1 } },
            2,
            4.25
        );

        var expectedResult = new ProductSearchResultDto(
            productDtos,
            2,
            1,
            10,
            1,
            facetsDto
        );

        _productQueryServiceMock
            .Setup(x => x.SearchProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.Facets.Should().NotBeNull();
        result.Facets.Categories.Should().ContainKey("Electronics");
    }

    [Fact]
    public async Task Handle_ShouldCallQueryService_WithCorrectQuery()
    {
        // Arrange
        var query = new SearchProductsQuery(
            "gaming laptop",
            CategoryId: Guid.NewGuid(),
            MinPrice: 500,
            MaxPrice: 2000,
            InStockOnly: true,
            FeaturedOnly: true,
            Tags: ["gaming", "high-performance"],
            MinRating: 4.0m,
            SortBy: "rating",
            Page: 1,
            PageSize: 20
        );

        var expectedResult = new ProductSearchResultDto(
            [],
            0,
            1,
            20,
            0,
            new ProductSearchFacetsDto([], [], [], 0, 0)
        );

        _productQueryServiceMock
            .Setup(x => x.SearchProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _productQueryServiceMock.Verify(
            x => x.SearchProductsAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenSuccessful()
    {
        // Arrange
        var query = new SearchProductsQuery("test query", Page: 1, PageSize: 10);

        var expectedResult = new ProductSearchResultDto(
            [],
            0,
            1,
            10,
            0,
            new ProductSearchFacetsDto([], [], [], 0, 0)
        );

        _productQueryServiceMock
            .Setup(x => x.SearchProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling SearchProductsQuery")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully searched products")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenServiceFails()
    {
        // Arrange
        var query = new SearchProductsQuery("test");
        var expectedException = new InvalidOperationException("Search service error");

        _productQueryServiceMock
            .Setup(x => x.SearchProductsAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Should().Be(expectedException);
    }
}