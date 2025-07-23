using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for Customer aggregate operations
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    /// <summary>
    /// Gets a customer by email address
    /// </summary>
    /// <param name="email">The customer's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The customer with the specified email, or null if not found</returns>
    Task<Customer?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by phone number
    /// </summary>
    /// <param name="phoneNumber">The customer's phone number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The customer with the specified phone number, or null if not found</returns>
    Task<Customer?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active customers (customers with active accounts)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active customers</returns>
    Task<IEnumerable<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inactive customers (customers with deactivated accounts)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of inactive customers</returns>
    Task<IEnumerable<Customer>> GetInactiveCustomersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers registered within a date range
    /// </summary>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of customers registered within the specified date range</returns>
    Task<IEnumerable<Customer>> GetByRegistrationDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers who were last active within a date range
    /// </summary>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of customers who were active within the specified date range</returns>
    Task<IEnumerable<Customer>> GetByLastActiveDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches customers by name (first name or last name)
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of customers matching the search term</returns>
    Task<IEnumerable<Customer>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers with pagination support
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of customers</returns>
    Task<(IEnumerable<Customer> Customers, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers by multiple identifiers
    /// </summary>
    /// <param name="customerIds">Collection of customer identifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of customers with the specified identifiers</returns>
    Task<IEnumerable<Customer>> GetByIdsAsync(
        IEnumerable<Guid> customerIds, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a customer with the specified email exists
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="excludeId">Optional customer ID to exclude from the check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a customer with the email exists, false otherwise</returns>
    Task<bool> ExistsByEmailAsync(
        Email email, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a customer with the specified phone number exists
    /// </summary>
    /// <param name="phoneNumber">The phone number</param>
    /// <param name="excludeId">Optional customer ID to exclude from the check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a customer with the phone number exists, false otherwise</returns>
    Task<bool> ExistsByPhoneNumberAsync(
        PhoneNumber phoneNumber, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers who haven't been active since the specified date
    /// </summary>
    /// <param name="cutoffDate">The cutoff date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of customers who haven't been active since the cutoff date</returns>
    Task<IEnumerable<Customer>> GetInactiveCustomersSinceAsync(
        DateTime cutoffDate, 
        CancellationToken cancellationToken = default);
}