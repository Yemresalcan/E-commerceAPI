using ECommerce.Application.Commands.Orders;
using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers.Orders;
using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Tests.Handlers.Orders;

public class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<PlaceOrderCommandHandler>> _loggerMock;
    private readonly PlaceOrderCommandHandler _handler;

    public PlaceOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<PlaceOrderCommandHandler>>();
        _handler = new PlaceOrderCommandHandler(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOrderAndReturnId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var customer = Customer.Create(
            "John",
            "Doe",
            new Email("test@example.com"),
            new PhoneNumber("+1234567890")
        );

        var product = Product.Create(
            "Test Product",
            "Test Description",
            new Money(50m, "USD"),
            "TEST-001",
            10,
            2,
            categoryId
        );

        var orderItems = new List<OrderItemDto>
        {
            new(productId, "Test Product", 2, 50m, "USD", 0)
        };

        var command = new PlaceOrderCommand(
            customerId,
            "123 Main St, City, State",
            "456 Billing St, City, State",
            orderItems
        );

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _customerRepositoryMock.Verify(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Exactly(2)); // Once for validation, once for stock update
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderItems = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), "Test Product", 1, 50m, "USD", 0)
        };

        var command = new PlaceOrderCommand(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Customer with ID '{customerId}' not found");
        _customerRepositoryMock.Verify(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var customer = Customer.Create(
            "John",
            "Doe",
            new Email("test@example.com"),
            new PhoneNumber("+1234567890")
        );

        var orderItems = new List<OrderItemDto>
        {
            new(productId, "Test Product", 1, 50m, "USD", 0)
        };

        var command = new PlaceOrderCommand(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Product with ID '{productId}' not found");
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var customer = Customer.Create(
            "John",
            "Doe",
            new Email("test@example.com"),
            new PhoneNumber("+1234567890")
        );

        var product = Product.Create(
            "Test Product",
            "Test Description",
            new Money(50m, "USD"),
            "TEST-001",
            5, // Only 5 in stock
            2,
            categoryId
        );

        var orderItems = new List<OrderItemDto>
        {
            new(productId, "Test Product", 10, 50m, "USD", 0) // Requesting 10
        };

        var command = new PlaceOrderCommand(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Insufficient stock");
        exception.Message.Should().Contain("Available: 5, Requested: 10");
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommandWithDiscount_ShouldCreateOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var customer = Customer.Create(
            "John",
            "Doe",
            new Email("test@example.com"),
            new PhoneNumber("+1234567890")
        );

        var product = Product.Create(
            "Test Product",
            "Test Description",
            new Money(100m, "USD"),
            "TEST-001",
            10,
            2,
            categoryId
        );

        var orderItems = new List<OrderItemDto>
        {
            new(productId, "Test Product", 1, 100m, "USD", 10m) // $10 discount
        };

        var command = new PlaceOrderCommand(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _orderRepositoryMock.Verify(x => x.AddAsync(It.Is<Order>(o => 
            o.TotalAmount.Amount == 90m), It.IsAny<CancellationToken>()), Times.Once);
    }
}