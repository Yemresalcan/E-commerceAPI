using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Infrastructure.Persistence.Interceptors;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>
/// Database configuration and dependency injection setup
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Adds Entity Framework Core services to the dependency injection container with performance optimizations
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string is not configured.");

        // Add connection pooling with enhanced performance optimizations
        services.AddDbContextPool<ECommerceDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ECommerceDbContext).Assembly.FullName);
                
                // Enhanced retry policy for better resilience
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);

                // Performance optimizations
                npgsqlOptions.CommandTimeout(30); // 30 seconds timeout
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                
                // Connection pooling optimizations
                npgsqlOptions.SetPostgresVersion(new Version(15, 0)); // Specify PostgreSQL version for optimizations
                
                // Additional performance optimizations
                // Note: EnableParameterLogging is not available in this version
            });

            // Performance optimizations
            options.EnableServiceProviderCaching();
            options.EnableSensitiveDataLogging(false); // Disable in production for performance
            
            // Configure query tracking behavior for better performance
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            
            // Enable compiled queries for better performance
            options.EnableServiceProviderCaching(true);
            
            // Configure interceptors for performance monitoring
            options.AddInterceptors(new PerformanceInterceptor());

            // Configure additional options based on environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.LogTo(Console.WriteLine, LogLevel.Information);
            }
            else
            {
                // Production optimizations
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
                
                // Enable query caching in production
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }, 
        poolSize: 256); // Increased connection pool size for better concurrency

        return services;
    }

    /// <summary>
    /// Ensures the database is created and migrations are applied
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task EnsureDatabaseCreatedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        await context.Database.MigrateAsync();
    }
}