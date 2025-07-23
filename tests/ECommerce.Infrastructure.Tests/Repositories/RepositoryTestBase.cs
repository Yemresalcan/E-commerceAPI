using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Tests.Repositories;

/// <summary>
/// Base class for repository integration tests using in-memory database
/// </summary>
public abstract class RepositoryTestBase : IAsyncLifetime
{
    protected ECommerceDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ECommerceDbContext(options);
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    protected async Task SeedDatabaseAsync()
    {
        // This method can be overridden in derived classes to provide specific test data
        await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Clears all data from the database
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        Context.Products.RemoveRange(Context.Products);
        Context.Orders.RemoveRange(Context.Orders);
        Context.Customers.RemoveRange(Context.Customers);
        Context.Categories.RemoveRange(Context.Categories);
        Context.ProductReviews.RemoveRange(Context.ProductReviews);
        Context.OrderItems.RemoveRange(Context.OrderItems);
        Context.Payments.RemoveRange(Context.Payments);
        Context.Addresses.RemoveRange(Context.Addresses);
        Context.Profiles.RemoveRange(Context.Profiles);
        
        await Context.SaveChangesAsync();
    }
}