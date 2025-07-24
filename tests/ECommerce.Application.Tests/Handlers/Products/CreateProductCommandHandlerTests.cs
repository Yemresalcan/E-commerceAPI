using ECommerce.Application.Commands.Products;
using ECommerce.Application.Handlers.Products;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Tests.Handlers.Products;

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
    public async Task Handle_ValidCommand_ShouldCreateProductAndReturnId()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid(),
            Weight: 1.5m,
            Dimensions: "10x10x10"
        );

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _productRepositoryMock.Verify(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid()
        );

        var existingProduct = Product.Create(
            "Existing Product",
            "Existing Description",
            new Domain.ValueObjects.Money(50m, "USD"),
            "TEST-001",
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

        exception.Message.Should().Contain("A product with SKU 'TEST-001' already exists");
        _productRepositoryMock.Verify(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ShouldCreateProduct()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Minimal Product",
            Description: "Minimal Description",
            Price: 10.00m,
            Currency: "EUR",
            Sku: "MIN-001",
            StockQuantity: 0,
            MinimumStockLevel: 0,
            CategoryId: Guid.NewGuid()
        );

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _productRepositoryMock.Verify(x => x.AddAsync(It.Is<Product>(p => 
            p.Name == command.Name && 
            p.Description == command.Description &&
            p.Sku == command.Sku &&
            p.StockQuantity == command.StockQuantity), It.IsAny<CancellationToken>()), Times.Once);
    }
}