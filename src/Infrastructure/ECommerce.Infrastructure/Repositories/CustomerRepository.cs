using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Customer repository implementation using Entity Framework Core
/// </summary>
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ECommerceDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a customer by email address
    /// </summary>
    public async Task<Customer?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        return await _dbSet
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .FirstOrDefaultAsync(c => c.Email.Value == email.Value, cancellationToken);
    }

    /// <summary>
    /// Gets a customer by phone number
    /// </summary>
    public async Task<Customer?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        return await _dbSet
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .FirstOrDefaultAsync(c => c.PhoneNumber != null && c.PhoneNumber.Value == phoneNumber.Value, cancellationToken);
    }

    /// <summary>
    /// Gets active customers (customers with active accounts)
    /// </summary>
    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets inactive customers (customers with deactivated accounts)
    /// </summary>
    public async Task<IEnumerable<Customer>> GetInactiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => !c.IsActive)
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets customers registered within a date range
    /// </summary>
    public async Task<IEnumerable<Customer>> GetByRegistrationDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.RegistrationDate >= startDate && c.RegistrationDate <= endDate)
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .OrderByDescending(c => c.RegistrationDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets customers who were last active within a date range
    /// </summary>
    public async Task<IEnumerable<Customer>> GetByLastActiveDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.LastActiveDate.HasValue && 
                       c.LastActiveDate.Value >= startDate && 
                       c.LastActiveDate.Value <= endDate)
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .OrderByDescending(c => c.LastActiveDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Searches customers by name (first name or last name)
    /// </summary>
    public async Task<IEnumerable<Customer>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return [];

        var lowerSearchTerm = searchTerm.ToLower();

        return await _dbSet
            .Where(c => c.FirstName.ToLower().Contains(lowerSearchTerm) || 
                       c.LastName.ToLower().Contains(lowerSearchTerm))
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets customers with pagination support
    /// </summary>
    public async Task<(IEnumerable<Customer> Customers, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var totalCount = await _dbSet.CountAsync(cancellationToken);
        
        var customers = await _dbSet
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (customers, totalCount);
    }

    /// <summary>
    /// Gets customers by multiple identifiers
    /// </summary>
    public async Task<IEnumerable<Customer>> GetByIdsAsync(
        IEnumerable<Guid> customerIds, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customerIds);

        var ids = customerIds.ToList();
        if (!ids.Any())
            return [];

        return await _dbSet
            .Where(c => ids.Contains(c.Id))
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a customer with the specified email exists
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(
        Email email, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        var query = _dbSet.Where(c => c.Email.Value == email.Value);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a customer with the specified phone number exists
    /// </summary>
    public async Task<bool> ExistsByPhoneNumberAsync(
        PhoneNumber phoneNumber, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        var query = _dbSet.Where(c => c.PhoneNumber != null && c.PhoneNumber.Value == phoneNumber.Value);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Gets customers who haven't been active since the specified date
    /// </summary>
    public async Task<IEnumerable<Customer>> GetInactiveCustomersSinceAsync(
        DateTime cutoffDate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => !c.LastActiveDate.HasValue || c.LastActiveDate.Value < cutoffDate)
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .OrderBy(c => c.LastActiveDate ?? c.RegistrationDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Override GetByIdAsync to include related entities
    /// </summary>
    public override async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Override GetAllAsync to include related entities
    /// </summary>
    public override async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Addresses)
            .Include(c => c.Profile)
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToListAsync(cancellationToken);
    }
}