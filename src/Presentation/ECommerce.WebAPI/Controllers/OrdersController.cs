using ECommerce.Application.Commands.Orders;
using ECommerce.Application.Queries.Orders;
using ECommerce.Application.DTOs;
using ECommerce.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace ECommerce.WebAPI.Controllers;

/// <summary>
/// Orders API controller providing order management operations
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get orders with pagination and filtering
    /// </summary>
    /// <param name="searchTerm">Search term for order details</param>
    /// <param name="customerId">Filter by customer ID</param>
    /// <param name="status">Filter by order status</param>
    /// <param name="minAmount">Minimum order amount filter</param>
    /// <param name="maxAmount">Maximum order amount filter</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <param name="paymentMethod">Filter by payment method</param>
    /// <param name="paymentStatus">Filter by payment status</param>
    /// <param name="sortBy">Sort criteria (created_desc, created_asc, amount_desc, amount_asc)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? status = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? paymentMethod = null,
        [FromQuery] string? paymentStatus = null,
        [FromQuery] string? sortBy = "created_desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            return BadRequest("Start date cannot be greater than end date");

        var query = new GetOrdersQuery(
            searchTerm,
            customerId,
            status,
            minAmount,
            maxAmount,
            startDate,
            endDate,
            paymentMethod,
            paymentStatus,
            sortBy,
            page,
            pageSize);

        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Order with ID {id} was not found");

        return Ok(result);
    }

    /// <summary>
    /// Place a new order
    /// </summary>
    /// <param name="command">Order placement data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<Guid>> PlaceOrder(
        [FromBody] PlaceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var orderId = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { id = orderId }, orderId);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="command">Order status update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.OrderId)
            return BadRequest("Order ID in URL does not match the ID in the request body");

        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="command">Order cancellation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] CancelOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.OrderId)
            return BadRequest("Order ID in URL does not match the ID in the request body");

        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="status">Filter by order status</param>
    /// <param name="startDate">Start date filter</param>
    /// <param name="endDate">End date filter</param>
    /// <param name="sortBy">Sort criteria</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of customer orders</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetCustomerOrders(
        Guid customerId,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = "created_desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            return BadRequest("Start date cannot be greater than end date");

        var query = new GetOrdersQuery(
            null, // searchTerm
            customerId,
            status,
            null, // minAmount
            null, // maxAmount
            startDate,
            endDate,
            null, // paymentMethod
            null, // paymentStatus
            sortBy,
            page,
            pageSize);

        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}