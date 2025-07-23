using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Queries;

/// <summary>
/// Test query to verify CQRS infrastructure is working
/// </summary>
/// <param name="Id">Test ID</param>
public record TestQuery(int Id) : IQuery<string>;

/// <summary>
/// Handler for TestQuery
/// </summary>
public class TestQueryHandler(ILogger<TestQueryHandler> logger) : IQueryHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing test query with ID: {Id}", request.Id);
        return Task.FromResult($"Query result for ID: {request.Id}");
    }
}