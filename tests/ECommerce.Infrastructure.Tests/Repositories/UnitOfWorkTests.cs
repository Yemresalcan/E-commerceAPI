using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.ValueObjects;
using ECommerce.Infrastructure.Repositories;

namespace ECommerce.Infrastructure.Tests.Repositories;

public class UnitOfWorkTests : RepositoryTestBase
{
    private UnitOfWork _unitOfWork = null!;
    private ProductRepository _productRepository = null!;

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _unitOfWork = new UnitOfWork(Context);
        _productRepository = new ProductRepository(Context);
    }

    public new async Task DisposeAsync()
    {
        _unitOfWork?.Dispose();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(100, "USD"), "SKU-001", 10, 2, categoryId);
        
        await _productRepository.AddAsync(product);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0); // Should return number of affected rows
        
        var savedProduct = await _productRepository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("Test Product");
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
    public async Task BeginTransactionAsync_WhenTransactionAlreadyActive_ShouldThrowException()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        // Act & Assert
        var act = async () => await _unitOfWork.BeginTransactionAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A transaction is already active*");
    }

    [Fact]
    public async Task CommitTransactionAsync_ShouldCommitChanges()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(100, "USD"), "SKU-001", 10, 2, categoryId);
        
        await _unitOfWork.BeginTransactionAsync();
        await _productRepository.AddAsync(product);

        // Act
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        _unitOfWork.HasActiveTransaction.Should().BeFalse();
        
        var savedProduct = await _productRepository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitTransactionAsync_WithoutActiveTransaction_ShouldThrowException()
    {
        // Act & Assert
        var act = async () => await _unitOfWork.CommitTransactionAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No active transaction to commit*");
    }

    [Fact]
    public async Task RollbackTransactionAsync_ShouldRollbackChanges()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(100, "USD"), "SKU-001", 10, 2, categoryId);
        
        await _unitOfWork.BeginTransactionAsync();
        await _productRepository.AddAsync(product);

        // Act
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        _unitOfWork.HasActiveTransaction.Should().BeFalse();
        
        var savedProduct = await _productRepository.GetByIdAsync(product.Id);
        savedProduct.Should().BeNull(); // Should not exist because transaction was rolled back
    }

    [Fact]
    public async Task RollbackTransactionAsync_WithoutActiveTransaction_ShouldThrowException()
    {
        // Act & Assert
        var act = async () => await _unitOfWork.RollbackTransactionAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No active transaction to rollback*");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithFunction_ShouldExecuteInTransaction()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(100, "USD"), "SKU-001", 10, 2, categoryId);

        // Act
        var result = await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
        {
            await _productRepository.AddAsync(product, ct);
            return product.Id;
        });

        // Assert
        result.Should().Be(product.Id);
        _unitOfWork.HasActiveTransaction.Should().BeFalse(); // Transaction should be completed
        
        var savedProduct = await _productRepository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithAction_ShouldExecuteInTransaction()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(100, "USD"), "SKU-001", 10, 2, categoryId);

        // Act
        await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
        {
            await _productRepository.AddAsync(product, ct);
        });

        // Assert
        _unitOfWork.HasActiveTransaction.Should().BeFalse(); // Transaction should be completed
        
        var savedProduct = await _productRepository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenExceptionThrown_ShouldRollbackTransaction()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(100, "USD"), "SKU-001", 10, 2, categoryId);

        // Act & Assert
        var act = async () => await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
        {
            await _productRepository.AddAsync(product, ct);
            throw new InvalidOperationException("Test exception");
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        _unitOfWork.HasActiveTransaction.Should().BeFalse(); // Transaction should be rolled back
        
        var savedProduct = await _productRepository.GetByIdAsync(product.Id);
        savedProduct.Should().BeNull(); // Should not exist because transaction was rolled back
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithExistingTransaction_ShouldNotStartNewTransaction()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", new Money(100, "USD"), "SKU-001", 10, 2, categoryId);
        
        await _unitOfWork.BeginTransactionAsync();
        var originalTransactionId = _unitOfWork.CurrentTransactionId;

        // Act
        await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
        {
            await _productRepository.AddAsync(product, ct);
        });

        // Assert
        _unitOfWork.HasActiveTransaction.Should().BeTrue(); // Original transaction should still be active
        _unitOfWork.CurrentTransactionId.Should().Be(originalTransactionId); // Same transaction ID
        
        // Commit the original transaction to verify the product was saved
        await _unitOfWork.CommitTransactionAsync();
        
        var savedProduct = await _productRepository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
    }

    [Fact]
    public async Task Dispose_ShouldDisposeTransaction()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();
        _unitOfWork.HasActiveTransaction.Should().BeTrue();

        // Act
        _unitOfWork.Dispose();

        // Assert
        _unitOfWork.HasActiveTransaction.Should().BeFalse();
    }

    [Fact]
    public async Task MultipleOperations_WithTransaction_ShouldMaintainConsistency()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = Product.Create("Product 1", "Description 1", new Money(100, "USD"), "SKU-001", 10, 2, categoryId);
        var product2 = Product.Create("Product 2", "Description 2", new Money(200, "USD"), "SKU-002", 5, 1, categoryId);

        // Act
        await _unitOfWork.BeginTransactionAsync();
        
        await _productRepository.AddAsync(product1);
        await _productRepository.AddAsync(product2);
        
        // Update product1
        product1.Update("Updated Product 1", "Updated Description", new Money(150, "USD"));
        _productRepository.Update(product1);
        
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var savedProduct1 = await _productRepository.GetByIdAsync(product1.Id);
        var savedProduct2 = await _productRepository.GetByIdAsync(product2.Id);
        
        savedProduct1.Should().NotBeNull();
        savedProduct1!.Name.Should().Be("Updated Product 1");
        savedProduct1.Price.Amount.Should().Be(150);
        
        savedProduct2.Should().NotBeNull();
        savedProduct2!.Name.Should().Be("Product 2");
    }
}