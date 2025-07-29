using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Persistence;
using ECommerce.ReadModel;
using ECommerce.WebAPI.Extensions;
using Serilog;

// Configure early Serilog logger for startup logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ECommerce Web API application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog with full configuration
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Configure services using modern .NET 9 patterns
    ConfigureServices(builder.Services, builder.Configuration);
    
    // Configure host options for graceful shutdown
    builder.Services.Configure<HostOptions>(options =>
    {
        options.ShutdownTimeout = TimeSpan.FromSeconds(30);
        options.ServicesStartConcurrently = true;
        options.ServicesStopConcurrently = false;
    });

    var app = builder.Build();

    // Configure middleware pipeline with proper ordering
    ConfigureMiddleware(app);

    // Configure infrastructure and startup tasks
    await ConfigureInfrastructureAsync(app);

    // Configure graceful shutdown
    ConfigureGracefulShutdown(app);

    Log.Information("ECommerce Web API started successfully on {Environment}", 
        builder.Environment.EnvironmentName);

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ECommerce Web API terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Configures all services in the DI container using modern .NET 9 patterns
static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Core ASP.NET Core services
    services.AddControllers(options =>
    {
        // Configure model binding and validation
        options.SuppressAsyncSuffixInActionNames = false;
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

    services.AddEndpointsApiExplorer();

    // Configure JSON options for modern .NET 9
    services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.SerializerOptions.WriteIndented = false;
        options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

    // Add comprehensive Swagger/OpenAPI documentation
    services.AddSwaggerDocumentation(configuration);

    // Configure strongly typed options
    services.AddConfigurationOptions(configuration);

    // Add application layer services
    services.AddApplicationServices(configuration);

    // Add database services with connection resilience
    services.AddDatabaseServices(configuration);

    // Add infrastructure services (messaging, caching, search)
    services.AddInfrastructureServices(configuration);

    // Configure comprehensive health checks
    services.AddComprehensiveHealthChecks(configuration);

    // Add CORS policy for development
    services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });

        options.AddPolicy("ProductionPolicy", policy =>
        {
            policy.WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                  .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-Api-Key")
                  .AllowCredentials();
        });
    });

    // Add response compression
    services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    });

    // Add response caching
    services.AddResponseCaching();

    // Add memory cache
    services.AddMemoryCache();

    // Configure request timeout (simplified for .NET 9)
    services.AddRequestTimeouts();

    // Add basic rate limiting (simplified for .NET 9)
    services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = 429;
    });
}

// Configures the middleware pipeline with proper ordering
static void ConfigureMiddleware(WebApplication app)
{
    // Add security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        await next();
    });

    // Add request timeout
    app.UseRequestTimeouts();

    // Add rate limiting
    app.UseRateLimiter();

    // Add request logging (should be early in pipeline)
    app.UseRequestLogging();

    // Add global exception handling
    app.UseGlobalExceptionHandling();

    // Configure Swagger UI (available in all environments for API documentation)
    app.UseSwaggerDocumentation(app.Services.GetService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>());

    // Enable static files for Swagger UI assets
    app.UseStaticFiles();

    // Add response compression
    app.UseResponseCompression();

    // Add response caching
    app.UseResponseCaching();

    // Configure CORS
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("DevelopmentPolicy");
    }
    else
    {
        app.UseCors("ProductionPolicy");
    }

    // HTTPS redirection
    app.UseHttpsRedirection();

    // Authentication and authorization (when implemented)
    // app.UseAuthentication();
    // app.UseAuthorization();

    // Map controllers
    app.MapControllers();

    // Map health check endpoints
    app.MapHealthCheckEndpoints();

    // Add a simple root endpoint
    app.MapGet("/", () => new
    {
        Service = "ECommerce API",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow,
        Documentation = "/api-docs",
        Health = "/health"
    }).WithTags("Root").WithOpenApi();
}

// Configures infrastructure services and performs startup tasks
static async Task ConfigureInfrastructureAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Ensure database is created and migrations are applied
    try
    {
        await DatabaseConfiguration.EnsureDatabaseCreatedAsync(scope.ServiceProvider);
        logger.LogInformation("Database configuration completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database configuration failed - continuing without database");
    }

    // Ensure Elasticsearch indices are created
    try
    {
        await scope.ServiceProvider.EnsureIndicesCreatedAsync();
        logger.LogInformation("Elasticsearch indices configuration completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Elasticsearch configuration failed - continuing without Elasticsearch");
    }

    // Configure infrastructure services
    try
    {
        await scope.ServiceProvider.ConfigureInfrastructureAsync();
        logger.LogInformation("Infrastructure services configuration completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Infrastructure services configuration failed - continuing with limited functionality");
    }
}

// Configures graceful shutdown handling
static void ConfigureGracefulShutdown(WebApplication app)
{
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    lifetime.ApplicationStarted.Register(() =>
    {
        logger.LogInformation("ECommerce Web API application started");
    });

    lifetime.ApplicationStopping.Register(() =>
    {
        logger.LogInformation("ECommerce Web API application is stopping");
    });

    lifetime.ApplicationStopped.Register(() =>
    {
        logger.LogInformation("ECommerce Web API application stopped");
    });


}

// Make Program class accessible for integration tests
public partial class Program { }