using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.ValueObjects;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Tests.Repositories;

/// <summary>
/// Simple tests for the UnitOfWork implementation
/// </summary>
public class SimpleUnitOfWorkTests : IAsyncLifetime
{
    private ECommerceDbContext _context = null!;
    private UnitOfWork _unitOfWork = null!;
    private Repository<Product> _repository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ECommerceDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _repository = new Repository<Product>(_context);
        
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _unitOfWork?.Dispose();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
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

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);
        
        var savedProduct = await _repository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldStartTransaction()
    {
        // Act
        await _unitOfWork.BeginTransactionAsync();

        // Assert
        _unitOfWork.HasActiveTransaction.Should().BeTrue();
        _unitOfWork.CurrentTransactionId.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitTransactionAsync_ShouldCommitChanges()
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

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(product);

        // Act
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        _unitOfWork.HasActiveTransaction.Should().BeFalse();
        
        var savedProduct = await _repository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
    }

    [Fact]
    public async Task RollbackTransactionAsync_ShouldRollbackChanges()
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

        await _unitOfWork.BeginTransactionAsync();
        await _repository.AddAsync(product);

        // Act
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        _unitOfWork.HasActiveTransaction.Should().BeFalse();
        
        var savedProduct = await _repository.GetByIdAsync(product.Id);
        savedProduct.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldExecuteInTransaction()
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
        var result = await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
        {
            await _repository.AddAsync(product, ct);
            return product.Id;
        });

        // Assert
        result.Should().Be(product.Id);
        _unitOfWork.HasActiveTransaction.Should().BeFalse();
        
        var savedProduct = await _repository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
    }
}