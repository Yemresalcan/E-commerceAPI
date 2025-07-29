using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers.Products;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Products;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Tests.Handlers;

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

    private static ProductDto CreateTestProductDto(string name = "Test Product", decimal price = 100m)
    {
        return new ProductDto(
            Guid.NewGuid(),
            name,
            "Test Description",
            "TEST-SKU",
            price,
            "USD",
            10,
            new CategoryDto(Guid.NewGuid(), "Test Category", "Category Description", null, "Test Category"),
            true,
            false,
            1.0m,
            "10x10x10",
            4.5m,
            5,
            true,
            false,
            DateTime.UtcNow,
            DateTime.UtcNow,
            new List<string>()
        );
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var query = new GetProductsQuery(
            SearchTerm: "test",
            Page: 1,
            PageSize: 10
        );

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto>
            {
                CreateTestProductDto("Test Product 1", 100m),
                CreateTestProductDto("Test Product 2", 200m)
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();

        _productQueryServiceMock.Verify(
            x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var query = new GetProductsQuery(SearchTerm: "nonexistent");

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldPassCorrectParameters()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetProductsQuery(
            CategoryId: categoryId,
            MinPrice: 50m,
            MaxPrice: 500m,
            InStockOnly: true,
            FeaturedOnly: false
        );

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _productQueryServiceMock.Verify(
            x => x.GetProductsAsync(
                It.Is<GetProductsQuery>(q => 
                    q.CategoryId == categoryId &&
                    q.MinPrice == 50m &&
                    q.MaxPrice == 500m &&
                    q.InStockOnly == true &&
                    q.FeaturedOnly == false),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPaginationParameters_ShouldPassCorrectParameters()
    {
        // Arrange
        var query = new GetProductsQuery(
            Page: 3,
            PageSize: 15,
            SortBy: "price"
        );

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto>(),
            TotalCount = 100,
            Page = 3,
            PageSize = 15
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Page.Should().Be(3);
        result.PageSize.Should().Be(15);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();

        _productQueryServiceMock.Verify(
            x => x.GetProductsAsync(
                It.Is<GetProductsQuery>(q => 
                    q.Page == 3 &&
                    q.PageSize == 15 &&
                    q.SortBy == "price"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithTagsAndRatingFilter_ShouldPassCorrectParameters()
    {
        // Arrange
        var tags = new List<string> { "electronics", "smartphone" };
        var query = new GetProductsQuery(
            Tags: tags,
            MinRating: 4.0m
        );

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _productQueryServiceMock.Verify(
            x => x.GetProductsAsync(
                It.Is<GetProductsQuery>(q => 
                    q.Tags != null &&
                    q.Tags.SequenceEqual(tags) &&
                    q.MinRating == 4.0m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var query = new GetProductsQuery();
        var expectedException = new InvalidOperationException("Service error");

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
        
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task Handle_ShouldLogInformationMessages()
    {
        // Arrange
        var query = new GetProductsQuery(SearchTerm: "test", Page: 1, PageSize: 10);

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto> { CreateTestProductDto() },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting product query")),
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
    public async Task Handle_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var query = new GetProductsQuery();
        var expectedException = new InvalidOperationException("Service error");

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while executing product query")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCorrectCancellationToken()
    {
        // Arrange
        var query = new GetProductsQuery();
        var cancellationToken = new CancellationToken();

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _productQueryServiceMock.Verify(
            x => x.GetProductsAsync(query, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullSearchTerm_ShouldHandleCorrectly()
    {
        // Arrange
        var query = new GetProductsQuery(SearchTerm: null);

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        _productQueryServiceMock.Verify(
            x => x.GetProductsAsync(
                It.Is<GetProductsQuery>(q => q.SearchTerm == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDefaultParameters_ShouldUseDefaults()
    {
        // Arrange
        var query = new GetProductsQuery(); // Using all defaults

        var expectedResult = new PagedResult<ProductDto>
        {
            Items = new List<ProductDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20
        };

        _productQueryServiceMock
            .Setup(x => x.GetProductsAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _productQueryServiceMock.Verify(
            x => x.GetProductsAsync(
                It.Is<GetProductsQuery>(q => 
                    q.SearchTerm == null &&
                    q.CategoryId == null &&
                    q.MinPrice == null &&
                    q.MaxPrice == null &&
                    q.InStockOnly == null &&
                    q.FeaturedOnly == null &&
                    q.Tags == null &&
                    q.MinRating == null &&
                    q.SortBy == "relevance" &&
                    q.Page == 1 &&
                    q.PageSize == 20),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}