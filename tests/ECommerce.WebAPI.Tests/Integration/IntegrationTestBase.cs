using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.Elasticsearch;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace ECommerce.WebAPI.Tests.Integration;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgreSqlContainer _postgresContainer;
    protected readonly RedisContainer _redisContainer;
    protected readonly ElasticsearchContainer _elasticsearchContainer;
    protected readonly RabbitMqContainer _rabbitMqContainer;
    protected WebApplicationFactory<Program> _factory = null!;
    protected HttpClient _client = null!;

    protected IntegrationTestBase()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("ecommerce_test")
            .WithUsername("test")
            .WithPassword("test")
            .WithPortBinding(5432, true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .Build();

        _elasticsearchContainer = new ElasticsearchBuilder()
            .WithImage("elasticsearch:8.11.0")
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithPortBinding(9200, true)
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start all containers in parallel
        var tasks = new[]
        {
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _elasticsearchContainer.StartAsync(),
            _rabbitMqContainer.StartAsync()
        };

        await Task.WhenAll(tasks);

        // Create the web application factory with test configuration
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString(),
                        ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString(),
                        ["Elasticsearch:Uri"] = _elasticsearchContainer.GetConnectionString(),
                        ["RabbitMQ:ConnectionString"] = _rabbitMqContainer.GetConnectionString(),
                        ["RabbitMQ:HostName"] = _rabbitMqContainer.Hostname,
                        ["RabbitMQ:Port"] = _rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                        ["RabbitMQ:UserName"] = "guest",
                        ["RabbitMQ:Password"] = "guest"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ECommerceDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add test database context
                    services.AddDbContext<ECommerceDbContext>(options =>
                    {
                        options.UseNpgsql(_postgresContainer.GetConnectionString());
                    });
                });

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                });
            });

        _client = _factory.CreateClient();

        // Initialize database
        await InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();

        // Stop all containers in parallel
        var tasks = new[]
        {
            _postgresContainer.DisposeAsync().AsTask(),
            _redisContainer.DisposeAsync().AsTask(),
            _elasticsearchContainer.DisposeAsync().AsTask(),
            _rabbitMqContainer.DisposeAsync().AsTask()
        };

        await Task.WhenAll(tasks);
    }

    private async Task InitializeDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        await context.Database.EnsureCreatedAsync();
        
        // Run any pending migrations
        await context.Database.MigrateAsync();
    }

    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    protected StringContent CreateJsonContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    protected async Task SeedDatabaseAsync<T>(params T[] entities) where T : class
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        await context.Set<T>().AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }

    protected async Task ClearDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
        
        // Clear all tables in reverse dependency order
        context.OrderItems.RemoveRange(context.OrderItems);
        context.Orders.RemoveRange(context.Orders);
        context.ProductReviews.RemoveRange(context.ProductReviews);
        context.Products.RemoveRange(context.Products);
        context.Categories.RemoveRange(context.Categories);
        context.Addresses.RemoveRange(context.Addresses);
        context.Profiles.RemoveRange(context.Profiles);
        context.Customers.RemoveRange(context.Customers);
        
        await context.SaveChangesAsync();
    }
}