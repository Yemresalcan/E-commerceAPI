using ECommerce.Application.Commands.Products;
using ECommerce.Application.Handlers.Products;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Tests.Handlers;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _loggerMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateProductCommandHandler>>();
        
        _handler = new CreateProductCommandHandler(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            100m,
            "USD",
            "TEST-001",
            10,
            5,
            Guid.NewGuid(),
            1.5m,
            "10x10x10"
        );

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        _productRepositoryMock.Verify(
            x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()),
            Times.Once);
        
        _productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProductWithCorrectProperties()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            100m,
            "USD",
            "TEST-001",
            10,
            5,
            Guid.NewGuid(),
            1.5m,
            "10x10x10"
        );

        Product? capturedProduct = null;
        
        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => capturedProduct = product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct!.Name.Should().Be(command.Name);
        capturedProduct.Description.Should().Be(command.Description);
        capturedProduct.Price.Amount.Should().Be(command.Price);
        capturedProduct.Price.Currency.Should().Be(command.Currency);
        capturedProduct.Sku.Should().Be(command.Sku);
        capturedProduct.StockQuantity.Should().Be(command.StockQuantity);
        capturedProduct.MinimumStockLevel.Should().Be(command.MinimumStockLevel);
        capturedProduct.CategoryId.Should().Be(command.CategoryId);
        capturedProduct.Weight.Should().Be(command.Weight);
        capturedProduct.Dimensions.Should().Be(command.Dimensions);
    }

    [Fact]
    public async Task Handle_WithExistingSku_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            100m,
            "USD",
            "EXISTING-SKU",
            10,
            5,
            Guid.NewGuid()
        );

        var existingProduct = Product.Create(
            "Existing Product",
            "Existing Description",
            new Money(50m, "USD"),
            "EXISTING-SKU",
            5,
            2,
            Guid.NewGuid()
        );

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain($"A product with SKU '{command.Sku}' already exists");
        
        _productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithMinimalParameters_ShouldCreateProductWithDefaults()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            100m,
            "USD",
            "TEST-001",
            10,
            5,
            Guid.NewGuid()
            // Weight and Dimensions will use defaults (0 and "")
        );

        Product? capturedProduct = null;
        
        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => capturedProduct = product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct!.Weight.Should().Be(0);
        capturedProduct.Dimensions.Should().Be("");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            100m,
            "USD",
            "TEST-001",
            10,
            5,
            Guid.NewGuid()
        );

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Be("Database error");
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            100m,
            "USD",
            "TEST-001",
            10,
            5,
            Guid.NewGuid()
        );

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Be("Save failed");
        
        _productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            100m,
            "USD",
            "TEST-001",
            10,
            5,
            Guid.NewGuid()
        );

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting product creation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully created product")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingSku_ShouldLogWarning()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            100m,
            "USD",
            "EXISTING-SKU",
            10,
            5,
            Guid.NewGuid()
        );

        var existingProduct = Product.Create(
            "Existing Product",
            "Existing Description",
            new Money(50m, "USD"),
            "EXISTING-SKU",
            5,
            2,
            Guid.NewGuid()
        );

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Product creation failed: SKU")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCorrectCancellationToken()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product",
            "Test Description",
            100m,
            "USD",
            "TEST-001",
            10,
            5,
            Guid.NewGuid()
        );

        var cancellationToken = new CancellationToken();

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _productRepositoryMock.Verify(
            x => x.GetBySkuAsync(command.Sku, cancellationToken),
            Times.Once);
        
        _productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), cancellationToken),
            Times.Once);
        
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once);
    }
}