using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers.Products;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Products;

namespace ECommerce.Application.Tests.Handlers.Products;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IProductQueryService> _productQueryServiceMock;
    private readonly Mock<ILogger<GetProductsQueryHandler>> _loggerMock;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _productQueryServiceMock = new Mock<IProductQueryService>();
        _loggerMock = new Mock<ILogger<GetProductsQueryHandler>>();
        _handler = new GetProductsQueryHandler(
            _productQueryServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenProductsExist()
    {
        // Arrange
        var query = new GetProductsQuery(
            SearchTerm: "test",
            Page: 1,
            PageSize: 10
        );

        var productDtos = new List<ProductDto>
        {
            new(Guid.NewGuid(), "Product 1", "", "", 100, "USD", 10, 
                new CategoryDto(Guid.NewGuid(), "Category", "", null, ""), 
                true, false, 0, "", 0, 0, true, false, DateTime.UtcNow, DateTime.UtcNow, []),
            new(Guid.NewGuid(), "Product 2", "", "", 200, "USD", 5, 
                new CategoryDto(Guid.NewGuid(), "Category", "", null, ""), 
                true, false, 0, "", 0, 0, true, false, DateTime.UtcNow, DateTime.UtcNow, [])
        };

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = productDtos,
            Page = 1,
            PageSize = 10,
            TotalCount = 2
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCallProductQueryService_WithCorrectQuery()
    {
        // Arrange
        var query = new GetProductsQuery(
            SearchTerm: "laptop",
            CategoryId: Guid.NewGuid(),
            MinPrice: 100,
            MaxPrice: 1000,
            InStockOnly: true,
            FeaturedOnly: false,
            Tags: ["electronics", "computers"],
            MinRating: 4.0m,
            SortBy: "price_asc",
            Page: 2,
            PageSize: 20
        );

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = [],
            Page = 2,
            PageSize = 20,
            TotalCount = 0
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _productQueryServiceMock.Verify(
            x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenSuccessful()
    {
        // Arrange
        var query = new GetProductsQuery(SearchTerm: "test", Page: 1, PageSize: 10);

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling GetProductsQuery")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenServiceThrows()
    {
        // Arrange
        var query = new GetProductsQuery(SearchTerm: "test");
        var expectedException = new InvalidOperationException("Service error");

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Should().Be(expectedException);
    }
}