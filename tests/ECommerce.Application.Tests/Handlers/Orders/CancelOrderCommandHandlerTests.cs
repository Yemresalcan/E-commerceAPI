using ECommerce.Application.Commands.Orders;
using ECommerce.Application.Handlers.Orders;
using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Tests.Handlers.Orders;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CancelOrderCommandHandler>> _loggerMock;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CancelOrderCommandHandler>>();
        _handler = new CancelOrderCommandHandler(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CancelPendingOrder_ShouldCancelOrderAndRestoreStock()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(productId, "Test Product", 2, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        var product = Product.Create(
            "Test Product",
            "Test Description",
            new Money(50m, "USD"),
            "TEST-001",
            8, // Current stock after order was placed (was 10, ordered 2)
            2,
            categoryId
        );

        var command = new CancelOrderCommand(orderId, "Customer requested cancellation");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        order.Status.Should().Be(OrderStatus.Cancelled);
        product.StockQuantity.Should().Be(10); // Stock restored (8 + 2)
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.Update(order), Times.Once);
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.Update(product), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CancelConfirmedOrder_ShouldCancelOrderAndRestoreStock()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(productId, "Test Product", 3, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );
        order.Confirm(); // Confirm the order

        var product = Product.Create(
            "Test Product",
            "Test Description",
            new Money(50m, "USD"),
            "TEST-001",
            7, // Current stock after order was placed (was 10, ordered 3)
            2,
            categoryId
        );

        var command = new CancelOrderCommand(orderId, "Out of stock from supplier");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        order.Status.Should().Be(OrderStatus.Cancelled);
        product.StockQuantity.Should().Be(10); // Stock restored (7 + 3)
        _productRepositoryMock.Verify(x => x.Update(product), Times.Once);
    }

    [Fact]
    public async Task Handle_CancelShippedOrder_ShouldCancelOrderButNotRestoreStock()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(productId, "Test Product", 2, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );
        order.Confirm();
        order.Ship(); // Ship the order

        var product = Product.Create(
            "Test Product",
            "Test Description",
            new Money(50m, "USD"),
            "TEST-001",
            8, // Current stock
            2,
            categoryId
        );

        var command = new CancelOrderCommand(orderId, "Damaged during shipping");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        order.Status.Should().Be(OrderStatus.Cancelled);
        product.StockQuantity.Should().Be(8); // Stock NOT restored for shipped orders
        _orderRepositoryMock.Verify(x => x.Update(order), Times.Once);
        _productRepositoryMock.Verify(x => x.Update(product), Times.Never); // Should not update product stock
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CancelOrderCommand(orderId, "Test reason");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Order with ID '{orderId}' not found");
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CancelOrderWithMultipleItems_ShouldRestoreStockForAllItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(productId1, "Product 1", 2, new Money(50m, "USD")),
            OrderItem.Create(productId2, "Product 2", 1, new Money(30m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        var product1 = Product.Create(
            "Product 1",
            "Description 1",
            new Money(50m, "USD"),
            "PROD-001",
            8, // Current stock (was 10, ordered 2)
            2,
            categoryId
        );

        var product2 = Product.Create(
            "Product 2",
            "Description 2",
            new Money(30m, "USD"),
            "PROD-002",
            4, // Current stock (was 5, ordered 1)
            1,
            categoryId
        );

        var command = new CancelOrderCommand(orderId, "Customer changed mind");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        order.Status.Should().Be(OrderStatus.Cancelled);
        product1.StockQuantity.Should().Be(10); // Stock restored (8 + 2)
        product2.StockQuantity.Should().Be(5);  // Stock restored (4 + 1)
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId1, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId2, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.Update(product1), Times.Once);
        _productRepositoryMock.Verify(x => x.Update(product2), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFoundDuringStockRestore_ShouldContinueWithOtherProducts()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(productId, "Test Product", 2, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        var command = new CancelOrderCommand(orderId, "Product discontinued");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null); // Product not found

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        order.Status.Should().Be(OrderStatus.Cancelled);
        _orderRepositoryMock.Verify(x => x.Update(order), Times.Once);
        _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}