using ECommerce.Application.Commands;
using ECommerce.Application.Queries;

namespace ECommerce.WebAPI.Controllers;

/// <summary>
/// Test controller to verify CQRS infrastructure
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Test command endpoint
    /// </summary>
    /// <param name="command">Test command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processed message</returns>
    [HttpPost("command")]
    public async Task<ActionResult<string>> TestCommand([FromBody] TestCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Test query endpoint
    /// </summary>
    /// <param name="id">Test ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query result</returns>
    [HttpGet("query/{id}")]
    public async Task<ActionResult<string>> TestQuery(int id, CancellationToken cancellationToken)
    {
        var query = new TestQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}