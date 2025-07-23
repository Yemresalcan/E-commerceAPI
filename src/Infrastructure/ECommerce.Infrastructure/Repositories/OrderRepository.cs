using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Order repository implementation using Entity Framework Core
/// </summary>
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ECommerceDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets orders by customer identifier
    /// </summary>
    public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets orders by status
    /// </summary>
    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets orders by customer and status
    /// </summary>
    public async Task<IEnumerable<Order>> GetByCustomerAndStatusAsync(
        Guid customerId, 
        OrderStatus status, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId && o.Status == status)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets orders within a date range
    /// </summary>
    public async Task<IEnumerable<Order>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets orders that contain a specific product
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersContainingProductAsync(
        Guid productId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.OrderItems.Any(oi => oi.ProductId == productId))
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets pending orders (orders that need processing)
    /// </summary>
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == OrderStatus.Pending)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderBy(o => o.CreatedAt) // Oldest first for processing
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets orders ready for shipping (confirmed orders)
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersReadyForShippingAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == OrderStatus.Confirmed)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderBy(o => o.CreatedAt) // Oldest first for shipping
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets shipped orders that haven't been delivered yet
    /// </summary>
    public async Task<IEnumerable<Order>> GetShippedOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == OrderStatus.Shipped)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderBy(o => o.UpdatedAt) // Oldest shipped first
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets orders with pagination support
    /// </summary>
    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var totalCount = await _dbSet.CountAsync(cancellationToken);
        
        var orders = await _dbSet
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (orders, totalCount);
    }

    /// <summary>
    /// Gets customer orders with pagination support
    /// </summary>
    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetCustomerOrdersPagedAsync(
        Guid customerId,
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var totalCount = await _dbSet
            .Where(o => o.CustomerId == customerId)
            .CountAsync(cancellationToken);
        
        var orders = await _dbSet
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (orders, totalCount);
    }

    /// <summary>
    /// Gets the most recent order for a customer
    /// </summary>
    public async Task<Order?> GetMostRecentOrderByCustomerAsync(
        Guid customerId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets order statistics for a customer
    /// </summary>
    public async Task<(int TotalOrders, decimal TotalAmount, DateTime? LastOrderDate)> GetCustomerOrderStatsAsync(
        Guid customerId, 
        CancellationToken cancellationToken = default)
    {
        var customerOrders = await _dbSet
            .Where(o => o.CustomerId == customerId && o.Status != OrderStatus.Cancelled)
            .Include(o => o.OrderItems)
            .ToListAsync(cancellationToken);

        if (!customerOrders.Any())
        {
            return (0, 0, null);
        }

        var totalOrders = customerOrders.Count;
        var totalAmount = customerOrders.Sum(o => o.TotalAmount.Amount);
        var lastOrderDate = customerOrders.Max(o => o.CreatedAt);

        return (totalOrders, totalAmount, lastOrderDate);
    }

    /// <summary>
    /// Override GetByIdAsync to include related entities
    /// </summary>
    public override async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    /// <summary>
    /// Override GetAllAsync to include related entities
    /// </summary>
    public override async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}