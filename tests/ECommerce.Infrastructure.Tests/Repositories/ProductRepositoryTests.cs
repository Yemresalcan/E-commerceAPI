using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.ValueObjects;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly ECommerceDbContext _context;
    private readonly ProductRepository _repository;
    private readonly Mock<ILogger<ProductRepository>> _productLoggerMock;
    private readonly Mock<ILogger<Repository<Product>>> _baseLoggerMock;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ECommerceDbContext(options);
        _productLoggerMock = new Mock<ILogger<ProductRepository>>();
        _baseLoggerMock = new Mock<ILogger<Repository<Product>>>();
        
        _repository = new ProductRepository(_context, _productLoggerMock.Object, _baseLoggerMock.Object);
    }

    [Fact]
    public async Task GetBySkuAsync_WithExistingSku_ShouldReturnProduct()
    {
        // Arrange
        var product = CreateTestProduct("TEST-001");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySkuAsync("TEST-001");

        // Assert
        result.Should().NotBeNull();
        result!.Sku.Should().Be("TEST-001");
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetBySkuAsync_WithNonExistentSku_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySkuAsync("NON-EXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetBySkuAsync_WithInvalidSku_ShouldReturnNull(string invalidSku)
    {
        // Act
        var result = await _repository.GetBySkuAsync(invalidSku);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySkuAsync_ShouldIncludeReviews()
    {
        // Arrange
        var product = CreateTestProduct("TEST-001");
        product.AddReview(Guid.NewGuid(), 5, "Great product", "Love it!");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySkuAsync("TEST-001");

        // Assert
        result.Should().NotBeNull();
        result!.Reviews.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByCategoryAsync_WithExistingCategory_ShouldReturnProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = CreateTestProduct("TEST-001", categoryId: categoryId);
        var product2 = CreateTestProduct("TEST-002", categoryId: categoryId);
        var product3 = CreateTestProduct("TEST-003", categoryId: Guid.NewGuid()); // Different category

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(categoryId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.CategoryId == categoryId);
    }

    [Fact]
    public async Task GetByCategoryAsync_WithNonExistentCategory_ShouldReturnEmpty()
    {
        // Arrange
        var product = CreateTestProduct("TEST-001");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveProductsAsync_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        var activeProduct1 = CreateTestProduct("ACTIVE-001");
        var activeProduct2 = CreateTestProduct("ACTIVE-002");
        var inactiveProduct = CreateTestProduct("INACTIVE-001");
        inactiveProduct.Deactivate();

        await _context.Products.AddRangeAsync(activeProduct1, activeProduct2, inactiveProduct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetFeaturedProductsAsync_ShouldReturnOnlyFeaturedAndActiveProducts()
    {
        // Arrange
        var featuredActiveProduct = CreateTestProduct("FEATURED-001");
        featuredActiveProduct.MarkAsFeatured();

        var featuredInactiveProduct = CreateTestProduct("FEATURED-002");
        featuredInactiveProduct.MarkAsFeatured();
        featuredInactiveProduct.Deactivate();

        var nonFeaturedProduct = CreateTestProduct("NORMAL-001");

        await _context.Products.AddRangeAsync(featuredActiveProduct, featuredInactiveProduct, nonFeaturedProduct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFeaturedProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(p => p.IsFeatured && p.IsActive);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_ShouldReturnProductsWithLowStock()
    {
        // Arrange
        var lowStockProduct1 = CreateTestProduct("LOW-001", stockQuantity: 5, minimumStockLevel: 10);
        var lowStockProduct2 = CreateTestProduct("LOW-002", stockQuantity: 10, minimumStockLevel: 10);
        var normalStockProduct = CreateTestProduct("NORMAL-001", stockQuantity: 20, minimumStockLevel: 10);
        var outOfStockProduct = CreateTestProduct("OUT-001", stockQuantity: 0, minimumStockLevel: 10);

        await _context.Products.AddRangeAsync(lowStockProduct1, lowStockProduct2, normalStockProduct, outOfStockProduct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.StockQuantity <= p.MinimumStockLevel && p.StockQuantity > 0);
    }

    [Fact]
    public async Task GetOutOfStockProductsAsync_ShouldReturnProductsWithZeroStock()
    {
        // Arrange
        var outOfStockProduct1 = CreateTestProduct("OUT-001", stockQuantity: 0);
        var outOfStockProduct2 = CreateTestProduct("OUT-002", stockQuantity: 0);
        var inStockProduct = CreateTestProduct("IN-001", stockQuantity: 10);

        await _context.Products.AddRangeAsync(outOfStockProduct1, outOfStockProduct2, inStockProduct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOutOfStockProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.StockQuantity <= 0);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingTerm_ShouldReturnMatchingProducts()
    {
        // Arrange
        var product1 = CreateTestProduct("SEARCH-001", name: "Smartphone Device");
        var product2 = CreateTestProduct("SEARCH-002", name: "Laptop Computer", description: "High-performance smartphone processor");
        var product3 = CreateTestProduct("SEARCH-003", name: "Tablet Device");
        var inactiveProduct = CreateTestProduct("SEARCH-004", name: "Smartphone Case");
        inactiveProduct.Deactivate();

        await _context.Products.AddRangeAsync(product1, product2, product3, inactiveProduct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("smartphone");

        // Assert
        result.Should().HaveCount(2); // Only active products matching the search term
        result.Should().OnlyContain(p => p.IsActive);
        result.Should().OnlyContain(p => 
            p.Name.ToLower().Contains("smartphone") || 
            p.Description.ToLower().Contains("smartphone"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchAsync_WithInvalidSearchTerm_ShouldReturnEmpty(string invalidTerm)
    {
        // Arrange
        var product = CreateTestProduct("TEST-001");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(invalidTerm);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPageAndCount()
    {
        // Arrange
        var products = Enumerable.Range(1, 25)
            .Select(i => CreateTestProduct($"TEST-{i:D3}", name: $"Product {i}"))
            .ToList();

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var (pagedProducts, totalCount) = await _repository.GetPagedAsync(2, 10);

        // Assert
        totalCount.Should().Be(25);
        pagedProducts.Should().HaveCount(10);
        pagedProducts.First().Name.Should().Be("Product 11"); // Ordered by name
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidPageParameters_ShouldUseDefaults()
    {
        // Arrange
        var products = Enumerable.Range(1, 5)
            .Select(i => CreateTestProduct($"TEST-{i:D3}"))
            .ToList();

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var (pagedProducts, totalCount) = await _repository.GetPagedAsync(0, -5);

        // Assert
        totalCount.Should().Be(5);
        pagedProducts.Should().HaveCount(5); // Should use default page size of 10, but only 5 products exist
    }

    [Fact]
    public async Task GetByIdsAsync_WithExistingIds_ShouldReturnMatchingProducts()
    {
        // Arrange
        var product1 = CreateTestProduct("TEST-001");
        var product2 = CreateTestProduct("TEST-002");
        var product3 = CreateTestProduct("TEST-003");

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        var idsToRetrieve = new[] { product1.Id, product3.Id };

        // Act
        var result = await _repository.GetByIdsAsync(idsToRetrieve);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == product1.Id);
        result.Should().Contain(p => p.Id == product3.Id);
        result.Should().NotContain(p => p.Id == product2.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WithEmptyIds_ShouldReturnEmpty()
    {
        // Arrange
        var product = CreateTestProduct("TEST-001");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdsAsync(Array.Empty<Guid>());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_WithNullIds_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _repository.GetByIdsAsync(null!));
    }

    [Fact]
    public async Task ExistsBySkuAsync_WithExistingSku_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateTestProduct("EXISTING-SKU");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsBySkuAsync("EXISTING-SKU");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsBySkuAsync_WithNonExistentSku_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsBySkuAsync("NON-EXISTENT");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsBySkuAsync_WithExcludeId_ShouldExcludeSpecifiedProduct()
    {
        // Arrange
        var product = CreateTestProduct("TEST-SKU");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsBySkuAsync("TEST-SKU", product.Id);

        // Assert
        result.Should().BeFalse(); // Should exclude the product with the same ID
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExistsBySkuAsync_WithInvalidSku_ShouldReturnFalse(string invalidSku)
    {
        // Act
        var result = await _repository.ExistsBySkuAsync(invalidSku);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeReviews()
    {
        // Arrange
        var product = CreateTestProduct("TEST-001");
        product.AddReview(Guid.NewGuid(), 5, "Great", "Excellent product");
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Reviews.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeReviews()
    {
        // Arrange
        var product1 = CreateTestProduct("TEST-001");
        product1.AddReview(Guid.NewGuid(), 5, "Great", "Excellent");
        
        var product2 = CreateTestProduct("TEST-002");
        product2.AddReview(Guid.NewGuid(), 4, "Good", "Nice product");

        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Reviews.Any());
    }

    private static Product CreateTestProduct(
        string sku, 
        string name = "Test Product", 
        string description = "Test Description",
        decimal price = 100m,
        string currency = "USD",
        int stockQuantity = 10,
        int minimumStockLevel = 5,
        Guid? categoryId = null)
    {
        return Product.Create(
            name,
            description,
            new Money(price, currency),
            sku,
            stockQuantity,
            minimumStockLevel,
            categoryId ?? Guid.NewGuid()
        );
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}