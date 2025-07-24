using ECommerce.Application.Commands.Products;
using ECommerce.Application.Handlers.Products;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Tests.Handlers.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateProductCommandHandler>> _loggerMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UpdateProductCommandHandler>>();
        _handler = new UpdateProductCommandHandler(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Product",
            Description: "Updated Description",
            Price: 149.99m,
            Currency: "USD",
            Weight: 2.5m,
            Dimensions: "15x15x15"
        );

        var existingProduct = Product.Create(
            "Original Product",
            "Original Description",
            new Money(99.99m, "USD"),
            "TEST-001",
            10,
            5,
            Guid.NewGuid()
        );

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(x => x.Update(It.IsAny<Product>()));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        existingProduct.Name.Should().Be(command.Name);
        existingProduct.Description.Should().Be(command.Description);
        existingProduct.Price.Amount.Should().Be(command.Price);
        existingProduct.Price.Currency.Should().Be(command.Currency);
        existingProduct.Weight.Should().Be(command.Weight);
        existingProduct.Dimensions.Should().Be(command.Dimensions);

        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.Update(existingProduct), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Updated Product",
            Description: "Updated Description",
            Price: 149.99m,
            Currency: "USD"
        );

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Product with ID '{productId}' not found");
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ShouldUpdateProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            ProductId: productId,
            Name: "Minimal Update",
            Description: "Minimal Description",
            Price: 25.00m,
            Currency: "EUR"
        );

        var existingProduct = Product.Create(
            "Original Product",
            "Original Description",
            new Money(99.99m, "USD"),
            "TEST-001",
            10,
            5,
            Guid.NewGuid()
        );

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(x => x.Update(It.IsAny<Product>()));

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        existingProduct.Name.Should().Be(command.Name);
        existingProduct.Description.Should().Be(command.Description);
        existingProduct.Price.Amount.Should().Be(command.Price);
        existingProduct.Price.Currency.Should().Be(command.Currency);
        existingProduct.Weight.Should().Be(0); // Default value
        existingProduct.Dimensions.Should().Be(""); // Default value
    }
}