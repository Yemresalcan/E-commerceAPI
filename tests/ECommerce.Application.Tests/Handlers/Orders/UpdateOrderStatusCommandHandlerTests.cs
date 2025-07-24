using ECommerce.Application.Commands.Orders;
using ECommerce.Application.Handlers.Orders;
using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Tests.Handlers.Orders;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateOrderStatusCommandHandler>> _loggerMock;
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UpdateOrderStatusCommandHandler>>();
        _handler = new UpdateOrderStatusCommandHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ConfirmPendingOrder_ShouldUpdateStatusToConfirmed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), "Test Product", 1, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        order.Status.Should().Be(OrderStatus.Confirmed);
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.Update(order), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShipConfirmedOrder_ShouldUpdateStatusToShipped()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), "Test Product", 1, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );
        order.Confirm(); // Set to confirmed first

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Shipped);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        order.Status.Should().Be(OrderStatus.Shipped);
        _orderRepositoryMock.Verify(x => x.Update(order), Times.Once);
    }

    [Fact]
    public async Task Handle_DeliverShippedOrder_ShouldUpdateStatusToDelivered()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), "Test Product", 1, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );
        order.Confirm();
        order.Ship(); // Set to shipped first

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Delivered);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        order.Status.Should().Be(OrderStatus.Delivered);
        _orderRepositoryMock.Verify(x => x.Update(order), Times.Once);
    }

    [Fact]
    public async Task Handle_CancelOrder_ShouldUpdateStatusToCancelled()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), "Test Product", 1, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Cancelled, "Customer requested cancellation");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        order.Status.Should().Be(OrderStatus.Cancelled);
        _orderRepositoryMock.Verify(x => x.Update(order), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

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
    public async Task Handle_InvalidStatusTransition_ShouldThrowException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), "Test Product", 1, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );

        // Try to ship a pending order (should be confirmed first)
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Shipped);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<ECommerce.Domain.Exceptions.InvalidOrderStateException>(
            () => _handler.Handle(command, CancellationToken.None));

        _orderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SetStatusToPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderItems = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), "Test Product", 1, new Money(50m, "USD"))
        };

        var order = Order.Create(
            customerId,
            "123 Main St",
            "456 Billing St",
            orderItems
        );
        order.Confirm();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Pending);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Cannot change order status back to Pending");
        _orderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}