using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation using Entity Framework Core
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(ECommerceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Indicates whether a transaction is currently active
    /// </summary>
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <summary>
    /// Gets the current transaction identifier
    /// </summary>
    public Guid? CurrentTransactionId => _currentTransaction?.TransactionId;

    /// <summary>
    /// Saves all changes made in this unit of work to the underlying data store
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle optimistic concurrency conflicts
            throw new InvalidOperationException(
                "The entity being updated was modified by another process. Please reload and try again.", 
                ex);
        }
        catch (DbUpdateException ex)
        {
            // Handle database update exceptions
            throw new InvalidOperationException(
                "An error occurred while saving changes to the database.", 
                ex);
        }
    }

    /// <summary>
    /// Begins a new transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already active. Complete the current transaction before starting a new one.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No active transaction to rollback.");
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Executes a function within a transaction scope
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="func">The function to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the function</returns>
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> func, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);

        var wasTransactionStartedHere = false;

        if (!HasActiveTransaction)
        {
            await BeginTransactionAsync(cancellationToken);
            wasTransactionStartedHere = true;
        }

        try
        {
            var result = await func(cancellationToken);

            if (wasTransactionStartedHere)
            {
                await CommitTransactionAsync(cancellationToken);
            }

            return result;
        }
        catch
        {
            if (wasTransactionStartedHere && HasActiveTransaction)
            {
                await RollbackTransactionAsync(cancellationToken);
            }
            throw;
        }
    }

    /// <summary>
    /// Executes an action within a transaction scope
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        await ExecuteInTransactionAsync(async (ct) =>
        {
            await action(ct);
            return true; // Dummy return value
        }, cancellationToken);
    }

    /// <summary>
    /// Disposes the current transaction
    /// </summary>
    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Disposes the unit of work and releases resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }

            _context.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer
    /// </summary>
    ~UnitOfWork()
    {
        Dispose(false);
    }
}