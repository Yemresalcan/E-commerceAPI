using ECommerce.Application.Commands.Customers;
using ECommerce.Application.Queries.Customers;
using ECommerce.Application.DTOs;
using ECommerce.Application.Common.Models;

namespace ECommerce.WebAPI.Controllers;

/// <summary>
/// Customers API controller providing customer management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get customers with pagination and filtering
    /// </summary>
    /// <param name="searchTerm">Search term for customer name or email</param>
    /// <param name="email">Filter by email address</param>
    /// <param name="phoneNumber">Filter by phone number</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="segment">Filter by customer segment</param>
    /// <param name="country">Filter by country</param>
    /// <param name="state">Filter by state</param>
    /// <param name="city">Filter by city</param>
    /// <param name="registrationStartDate">Registration start date filter</param>
    /// <param name="registrationEndDate">Registration end date filter</param>
    /// <param name="minLifetimeValue">Minimum lifetime value filter</param>
    /// <param name="maxLifetimeValue">Maximum lifetime value filter</param>
    /// <param name="minOrders">Minimum number of orders filter</param>
    /// <param name="maxOrders">Maximum number of orders filter</param>
    /// <param name="preferredLanguage">Filter by preferred language</param>
    /// <param name="sortBy">Sort criteria (registration_desc, registration_asc, name_asc, name_desc, lifetime_value_desc)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of customers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetCustomers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? email = null,
        [FromQuery] string? phoneNumber = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? segment = null,
        [FromQuery] string? country = null,
        [FromQuery] string? state = null,
        [FromQuery] string? city = null,
        [FromQuery] DateTime? registrationStartDate = null,
        [FromQuery] DateTime? registrationEndDate = null,
        [FromQuery] decimal? minLifetimeValue = null,
        [FromQuery] decimal? maxLifetimeValue = null,
        [FromQuery] int? minOrders = null,
        [FromQuery] int? maxOrders = null,
        [FromQuery] string? preferredLanguage = null,
        [FromQuery] string? sortBy = "registration_desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest("Page must be greater than 0");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        if (registrationStartDate.HasValue && registrationEndDate.HasValue && registrationStartDate > registrationEndDate)
            return BadRequest("Registration start date cannot be greater than end date");

        var query = new GetCustomersQuery(
            searchTerm,
            email,
            phoneNumber,
            isActive,
            segment,
            country,
            state,
            city,
            registrationStartDate,
            registrationEndDate,
            minLifetimeValue,
            maxLifetimeValue,
            minOrders,
            maxOrders,
            preferredLanguage,
            sortBy,
            page,
            pageSize);

        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Customer with ID {id} was not found");

        return Ok(result);
    }

    /// <summary>
    /// Register a new customer
    /// </summary>
    /// <param name="command">Customer registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created customer ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<Guid>> RegisterCustomer(
        [FromBody] RegisterCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var customerId = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id = customerId }, customerId);
    }

    /// <summary>
    /// Update customer profile
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="command">Customer profile update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id:guid}/profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateCustomerProfile(
        Guid id,
        [FromBody] UpdateCustomerProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.CustomerId)
            return BadRequest("Customer ID in URL does not match the ID in the request body");

        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Add a new address to a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="command">Address data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created address ID</returns>
    [HttpPost("{id:guid}/addresses")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<Guid>> AddCustomerAddress(
        Guid id,
        [FromBody] AddCustomerAddressCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.CustomerId)
            return BadRequest("Customer ID in URL does not match the ID in the request body");

        var addressId = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id }, addressId);
    }

    /// <summary>
    /// Search customers by email
    /// </summary>
    /// <param name="email">Email address to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer matching the email</returns>
    [HttpGet("search/email")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> SearchCustomerByEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email parameter is required");

        var query = new GetCustomersQuery(
            Email: email,
            Page: 1,
            PageSize: 1);

        var result = await mediator.Send(query, cancellationToken);

        if (!result.Items.Any())
            return NotFound($"Customer with email {email} was not found");

        return Ok(result.Items.First());
    }

    /// <summary>
    /// Get customer statistics summary
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer statistics</returns>
    [HttpGet("{id:guid}/statistics")]
    [ProducesResponseType(typeof(CustomerStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerStatisticsDto>> GetCustomerStatistics(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Customer with ID {id} was not found");

        return Ok(result.Statistics);
    }
}