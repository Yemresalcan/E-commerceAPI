using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Customers;

namespace ECommerce.Application.Handlers.Customers;

/// <summary>
/// Handler for GetCustomerQuery
/// </summary>
public class GetCustomerQueryHandler(
    ICustomerQueryService customerQueryService,
    ILogger<GetCustomerQueryHandler> logger)
    : IRequestHandler<GetCustomerQuery, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetCustomerQuery for customer ID: {CustomerId}", request.CustomerId);

        try
        {
            var customerDto = await customerQueryService.GetCustomerByIdAsync(request.CustomerId, cancellationToken);
            
            if (customerDto == null)
            {
                logger.LogWarning("Customer with ID {CustomerId} not found", request.CustomerId);
                return null;
            }

            logger.LogInformation("Successfully retrieved customer {CustomerId}", request.CustomerId);

            return customerDto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while handling GetCustomerQuery for customer ID: {CustomerId}", request.CustomerId);
            throw;
        }
    }
}