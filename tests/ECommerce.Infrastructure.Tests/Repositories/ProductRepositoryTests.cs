using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.ValueObjects;
using ECommerce.Infrastructure.Repositories;

namespace ECommerce.Infrastructure.Tests.Repositories;

public class ProductRepositoryTests : RepositoryTestBase
{
    private ProductRepository _repository = null!;

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new ProductRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Test Product",
            "Test Description",
            new Money(99.99m, "USD"),
            "TEST-SKU-001",
            10,
            2,
            categoryId);

        // Act
        await _repository.AddAsync(product);
        await Context.SaveChangesAsync();

        // Assert
        var savedProduct = await _repository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("Test Product");
        savedProduct.Sku.Should().Be("TEST-SKU-001");
        savedProduct.Price.Amount.Should().Be(99.99m);
        savedProduct.Price.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetBySkuAsync_ShouldReturnProductWithMatchingSku()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Test Product",
            "Test Description",
            new Money(99.99m, "USD"),
            "UNIQUE-SKU-001",
            10,
            2,
            categoryId);

        await _repository.AddAsync(product);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySkuAsync("UNIQUE-SKU-001");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Sku.Should().Be("UNIQUE-SKU-001");
    }

    [Fact]
    public async Task GetBySkuAsync_WithNonExistentSku_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySkuAsync("NON-EXISTENT-SKU");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnProductsInCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var otherCategoryId = Guid.NewGuid();

        var product1 = Product.Create("Product 1", "Description 1", new Money(10, "USD"), "SKU-001", 5, 1, categoryId);
        var product2 = Product.Create("Product 2", "Description 2", new Money(20, "USD"), "SKU-002", 5, 1, categoryId);
        var product3 = Product.Create("Product 3", "Description 3", new Money(30, "USD"), "SKU-003", 5, 1, otherCategoryId);

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(categoryId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == product1.Id);
        result.Should().Contain(p => p.Id == product2.Id);
        result.Should().NotContain(p => p.Id == product3.Id);
    }

    [Fact]
    public async Task GetActiveProductsAsync_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var activeProduct = Product.Create("Active Product", "Description", new Money(10, "USD"), "SKU-001", 5, 1, categoryId);
        var inactiveProduct = Product.Create("Inactive Product", "Description", new Money(20, "USD"), "SKU-002", 5, 1, categoryId);
        
        inactiveProduct.Deactivate();

        await _repository.AddAsync(activeProduct);
        await _repository.AddAsync(inactiveProduct);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Id == activeProduct.Id);
        result.Should().NotContain(p => p.Id == inactiveProduct.Id);
    }

    [Fact]
    public async Task GetFeaturedProductsAsync_ShouldReturnOnlyFeaturedAndActiveProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var featuredProduct = Product.Create("Featured Product", "Description", new Money(10, "USD"), "SKU-001", 5, 1, categoryId);
        var regularProduct = Product.Create("Regular Product", "Description", new Money(20, "USD"), "SKU-002", 5, 1, categoryId);
        var inactiveFeaturedProduct = Product.Create("Inactive Featured", "Description", new Money(30, "USD"), "SKU-003", 5, 1, categoryId);
        
        featuredProduct.MarkAsFeatured();
        inactiveFeaturedProduct.MarkAsFeatured();
        inactiveFeaturedProduct.Deactivate();

        await _repository.AddAsync(featuredProduct);
        await _repository.AddAsync(regularProduct);
        await _repository.AddAsync(inactiveFeaturedProduct);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFeaturedProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Id == featuredProduct.Id);
        result.Should().NotContain(p => p.Id == regularProduct.Id);
        result.Should().NotContain(p => p.Id == inactiveFeaturedProduct.Id);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_ShouldReturnProductsWithLowStock()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var lowStockProduct = Product.Create("Low Stock", "Description", new Money(10, "USD"), "SKU-001", 2, 5, categoryId); // Stock 2, Min 5
        var normalStockProduct = Product.Create("Normal Stock", "Description", new Money(20, "USD"), "SKU-002", 10, 5, categoryId); // Stock 10, Min 5
        var outOfStockProduct = Product.Create("Out of Stock", "Description", new Money(30, "USD"), "SKU-003", 0, 5, categoryId); // Stock 0, Min 5

        await _repository.AddAsync(lowStockProduct);
        await _repository.AddAsync(normalStockProduct);
        await _repository.AddAsync(outOfStockProduct);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Id == lowStockProduct.Id);
        result.Should().NotContain(p => p.Id == normalStockProduct.Id);
        result.Should().NotContain(p => p.Id == outOfStockProduct.Id); // Out of stock products are not included in low stock
    }

    [Fact]
    public async Task GetOutOfStockProductsAsync_ShouldReturnProductsWithZeroStock()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var inStockProduct = Product.Create("In Stock", "Description", new Money(10, "USD"), "SKU-001", 5, 2, categoryId);
        var outOfStockProduct = Product.Create("Out of Stock", "Description", new Money(20, "USD"), "SKU-002", 0, 2, categoryId);

        await _repository.AddAsync(inStockProduct);
        await _repository.AddAsync(outOfStockProduct);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOutOfStockProductsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Id == outOfStockProduct.Id);
        result.Should().NotContain(p => p.Id == inStockProduct.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnProductsMatchingSearchTerm()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = Product.Create("Gaming Laptop", "High performance gaming laptop", new Money(1000, "USD"), "SKU-001", 5, 1, categoryId);
        var product2 = Product.Create("Office Laptop", "Business laptop for office work", new Money(800, "USD"), "SKU-002", 5, 1, categoryId);
        var product3 = Product.Create("Gaming Mouse", "RGB gaming mouse", new Money(50, "USD"), "SKU-003", 10, 2, categoryId);

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("gaming");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == product1.Id);
        result.Should().Contain(p => p.Id == product3.Id);
        result.Should().NotContain(p => p.Id == product2.Id);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPageAndTotalCount()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>();
        
        for (int i = 1; i <= 15; i++)
        {
            var product = Product.Create($"Product {i:D2}", "Description", new Money(i * 10, "USD"), $"SKU-{i:D3}", 5, 1, categoryId);
            products.Add(product);
            await _repository.AddAsync(product);
        }
        
        await Context.SaveChangesAsync();

        // Act
        var (pagedProducts, totalCount) = await _repository.GetPagedAsync(2, 5); // Page 2, 5 items per page

        // Assert
        totalCount.Should().Be(15);
        pagedProducts.Should().HaveCount(5);
        
        // Products should be ordered by name, so page 2 should contain products 6-10
        var productList = pagedProducts.OrderBy(p => p.Name).ToList();
        productList[0].Name.Should().Be("Product 06");
        productList[4].Name.Should().Be("Product 10");
    }

    [Fact]
    public async Task ExistsBySkuAsync_WithExistingSku_ShouldReturnTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(10, "USD"), "EXISTING-SKU", 5, 1, categoryId);
        
        await _repository.AddAsync(product);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsBySkuAsync("EXISTING-SKU");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsBySkuAsync_WithNonExistentSku_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsBySkuAsync("NON-EXISTENT-SKU");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsBySkuAsync_WithExcludeId_ShouldExcludeSpecifiedProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(10, "USD"), "TEST-SKU", 5, 1, categoryId);
        
        await _repository.AddAsync(product);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsBySkuAsync("TEST-SKU", product.Id);

        // Assert
        result.Should().BeFalse(); // Should return false because we're excluding the product with this SKU
    }

    [Fact]
    public async Task Update_ShouldUpdateProductInDatabase()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Original Name", "Original Description", new Money(100, "USD"), "SKU-001", 5, 1, categoryId);
        
        await _repository.AddAsync(product);
        await Context.SaveChangesAsync();

        // Act
        product.Update("Updated Name", "Updated Description", new Money(150, "USD"));
        _repository.Update(product);
        await Context.SaveChangesAsync();

        // Assert
        var updatedProduct = await _repository.GetByIdAsync(product.Id);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("Updated Name");
        updatedProduct.Description.Should().Be("Updated Description");
        updatedProduct.Price.Amount.Should().Be(150);
    }

    [Fact]
    public async Task Delete_ShouldRemoveProductFromDatabase()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(10, "USD"), "SKU-001", 5, 1, categoryId);
        
        await _repository.AddAsync(product);
        await Context.SaveChangesAsync();

        // Act
        _repository.Delete(product);
        await Context.SaveChangesAsync();

        // Assert
        var deletedProduct = await _repository.GetByIdAsync(product.Id);
        deletedProduct.Should().BeNull();
    }
}