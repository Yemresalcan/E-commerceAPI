using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.ValueObjects;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests for the base Repository<T> implementation
/// </summary>
public class BaseRepositoryTests : IAsyncLifetime
{
    private ECommerceDbContext _context = null!;
    private Repository<Product> _repository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ECommerceDbContext(options);
        _repository = new Repository<Product>(_context);
        
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToContext()
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
        await _context.SaveChangesAsync();

        // Assert
        var savedProduct = await _repository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnEntity()
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

        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
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

        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(product.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ShouldModifyEntity()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Original Name",
            "Original Description",
            new Money(100m, "USD"),
            "TEST-SKU-001",
            10,
            2,
            categoryId);

        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        product.Update("Updated Name", "Updated Description", new Money(150m, "USD"));
        _repository.Update(product);
        await _context.SaveChangesAsync();

        // Assert
        var updatedProduct = await _repository.GetByIdAsync(product.Id);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("Updated Name");
        updatedProduct.Description.Should().Be("Updated Description");
        updatedProduct.Price.Amount.Should().Be(150m);
    }

    [Fact]
    public async Task Delete_ShouldRemoveEntity()
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

        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(product);
        await _context.SaveChangesAsync();

        // Assert
        var deletedProduct = await _repository.GetByIdAsync(product.Id);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldRemoveEntity()
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

        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteByIdAsync(product.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deletedProduct = await _repository.GetByIdAsync(product.Id);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = Product.Create("Product 1", "Description 1", new Money(10m, "USD"), "SKU-001", 5, 1, categoryId);
        var product2 = Product.Create("Product 2", "Description 2", new Money(20m, "USD"), "SKU-002", 5, 1, categoryId);
        var product3 = Product.Create("Product 3", "Description 3", new Money(30m, "USD"), "SKU-003", 5, 1, categoryId);

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(p => p.Id == product1.Id);
        result.Should().Contain(p => p.Id == product2.Id);
        result.Should().Contain(p => p.Id == product3.Id);
    }
}