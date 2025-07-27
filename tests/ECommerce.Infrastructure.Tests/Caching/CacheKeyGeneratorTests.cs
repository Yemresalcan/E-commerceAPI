using ECommerce.Infrastructure.Caching;

namespace ECommerce.Infrastructure.Tests.Caching;

public class CacheKeyGeneratorTests
{
    [Fact]
    public void ProductsList_WithBasicParameters_ShouldGenerateCorrectKey()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        // Act
        var result = CacheKeyGenerator.ProductsList(page, pageSize);

        // Assert
        result.Should().Be("products:list:1:10");
    }

    [Fact]
    public void ProductsList_WithSearchTerm_ShouldIncludeSearchInKey()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var searchTerm = "Laptop";

        // Act
        var result = CacheKeyGenerator.ProductsList(page, pageSize, searchTerm);

        // Assert
        result.Should().Be("products:list:1:10:search:laptop");
    }

    [Fact]
    public void ProductsList_WithCategoryId_ShouldIncludeCategoryInKey()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var categoryId = Guid.NewGuid();

        // Act
        var result = CacheKeyGenerator.ProductsList(page, pageSize, categoryId: categoryId);

        // Assert
        result.Should().Be($"products:list:1:10:category:{categoryId}");
    }

    [Fact]
    public void ProductsList_WithSearchTermAndCategory_ShouldIncludeBothInKey()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var searchTerm = "Gaming";
        var categoryId = Guid.NewGuid();

        // Act
        var result = CacheKeyGenerator.ProductsList(page, pageSize, searchTerm, categoryId);

        // Assert
        result.Should().Be($"products:list:1:10:search:gaming:category:{categoryId}");
    }

    [Fact]
    public void ProductsSearch_ShouldGenerateCorrectKey()
    {
        // Arrange
        var searchTerm = "Gaming Laptop";
        var page = 2;
        var pageSize = 20;

        // Act
        var result = CacheKeyGenerator.ProductsSearch(searchTerm, page, pageSize);

        // Assert
        result.Should().Be("products:search:gaming laptop:2:20");
    }

    [Fact]
    public void Product_ShouldGenerateCorrectKey()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var result = CacheKeyGenerator.Product(productId);

        // Assert
        result.Should().Be($"product:{productId}");
    }

    [Fact]
    public void OrdersList_ShouldGenerateCorrectKey()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;

        // Act
        var result = CacheKeyGenerator.OrdersList(customerId, page, pageSize);

        // Assert
        result.Should().Be($"orders:list:{customerId}:1:10");
    }

    [Fact]
    public void Order_ShouldGenerateCorrectKey()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var result = CacheKeyGenerator.Order(orderId);

        // Assert
        result.Should().Be($"order:{orderId}");
    }

    [Fact]
    public void CustomersList_WithBasicParameters_ShouldGenerateCorrectKey()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        // Act
        var result = CacheKeyGenerator.CustomersList(page, pageSize);

        // Assert
        result.Should().Be("customers:list:1:10");
    }

    [Fact]
    public void CustomersList_WithSearchTerm_ShouldIncludeSearchInKey()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var searchTerm = "John";

        // Act
        var result = CacheKeyGenerator.CustomersList(page, pageSize, searchTerm);

        // Assert
        result.Should().Be("customers:list:1:10:search:john");
    }

    [Fact]
    public void Customer_ShouldGenerateCorrectKey()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var result = CacheKeyGenerator.Customer(customerId);

        // Assert
        result.Should().Be($"customer:{customerId}");
    }

    [Fact]
    public void ProductsPattern_ShouldGenerateCorrectPattern()
    {
        // Act
        var result = CacheKeyGenerator.ProductsPattern();

        // Assert
        result.Should().Be("products:*");
    }

    [Fact]
    public void OrdersPattern_WithoutCustomerId_ShouldGenerateCorrectPattern()
    {
        // Act
        var result = CacheKeyGenerator.OrdersPattern();

        // Assert
        result.Should().Be("orders:*");
    }

    [Fact]
    public void OrdersPattern_WithCustomerId_ShouldGenerateCorrectPattern()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var result = CacheKeyGenerator.OrdersPattern(customerId);

        // Assert
        result.Should().Be($"orders:*:{customerId}:*");
    }

    [Fact]
    public void CustomersPattern_ShouldGenerateCorrectPattern()
    {
        // Act
        var result = CacheKeyGenerator.CustomersPattern();

        // Assert
        result.Should().Be("customers:*");
    }
}