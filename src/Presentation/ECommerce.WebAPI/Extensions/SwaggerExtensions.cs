using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json;

namespace ECommerce.WebAPI.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds comprehensive Swagger/OpenAPI documentation services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        // Add API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("version"),
                new HeaderApiVersionReader("X-Version"),
                new UrlSegmentApiVersionReader()
            );
        }).AddApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });



        // Configure Swagger generation
        services.AddSwaggerGen(options =>
        {
            // API Information
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1.0",
                Title = "ECommerce API",
                Description = "A comprehensive .NET 9 e-commerce API built with DDD, CQRS, and Clean Architecture patterns",
                Contact = new OpenApiContact
                {
                    Name = "ECommerce API Support",
                    Email = "support@ecommerce-api.com",
                    Url = new Uri("https://github.com/ecommerce-api/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://ecommerce-api.com/terms")
            });

            // Include XML documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Include XML comments from referenced projects
            var applicationXmlFile = "ECommerce.Application.xml";
            var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
            if (File.Exists(applicationXmlPath))
            {
                options.IncludeXmlComments(applicationXmlPath);
            }

            var domainXmlFile = "ECommerce.Domain.xml";
            var domainXmlPath = Path.Combine(AppContext.BaseDirectory, domainXmlFile);
            if (File.Exists(domainXmlPath))
            {
                options.IncludeXmlComments(domainXmlPath);
            }

            // Configure security definitions for future authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key needed to access the endpoints. X-Api-Key: My_API_Key",
                In = ParameterLocation.Header,
                Name = "X-Api-Key",
                Type = SecuritySchemeType.ApiKey
            });

            // Global security requirement (can be overridden per endpoint)
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Custom operation filters
            options.OperationFilter<SwaggerDefaultValues>();
            options.OperationFilter<SwaggerExcludeFilter>();

            // Document filters
            options.DocumentFilter<SwaggerDocumentFilter>();

            // Schema filters for better model documentation
            options.SchemaFilter<SwaggerSchemaFilter>();

            // Enable annotations
            options.EnableAnnotations();

            // Configure servers
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (environment == "Development")
            {
                options.AddServer(new OpenApiServer
                {
                    Url = "https://localhost:7001",
                    Description = "Development Server (HTTPS)"
                });
                options.AddServer(new OpenApiServer
                {
                    Url = "http://localhost:5001",
                    Description = "Development Server (HTTP)"
                });
            }
            else
            {
                options.AddServer(new OpenApiServer
                {
                    Url = "https://api.ecommerce.com",
                    Description = "Production Server"
                });
            }

            // Group endpoints by tags
            options.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                var controllerName = api.ActionDescriptor.RouteValues["controller"];
                return new[] { controllerName ?? "Unknown" };
            });

            options.DocInclusionPredicate((name, api) => true);
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="provider">API version description provider</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IApiVersionDescriptionProvider? provider = null)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            // Configure Swagger UI
            options.RoutePrefix = "api-docs";
            options.DocumentTitle = "ECommerce API Documentation";
            
            // Add API versions
            if (provider != null)
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/api-docs/{description.GroupName}/swagger.json",
                        $"ECommerce API {description.GroupName.ToUpperInvariant()}");
                }
            }
            else
            {
                options.SwaggerEndpoint("/api-docs/v1/swagger.json", "ECommerce API V1");
            }

            // UI Configuration
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelExpandDepth(2);
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
            options.EnableValidator();
            
            // Custom CSS for branding
            options.InjectStylesheet("/swagger-ui/custom.css");
            
            // Custom JavaScript
            options.InjectJavascript("/swagger-ui/custom.js");

            // OAuth configuration (for future use)
            options.OAuthClientId("ecommerce-api-swagger");
            options.OAuthAppName("ECommerce API - Swagger");
            options.OAuthUsePkce();
        });

        return app;
    }
}

/// <summary>
/// Swagger operation filter to set default values and improve documentation
/// </summary>
public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        // Check if the API is deprecated (simplified check)
        operation.Deprecated = apiDescription.ActionDescriptor.EndpointMetadata
            .Any(m => m.GetType().Name.Contains("Obsolete"));

        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                if (responseType.ApiResponseFormats.All(x => x.MediaType != contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }

        if (operation.Parameters == null)
            return;

        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
            parameter.Description ??= description.ModelMetadata?.Description;

            if (parameter.Schema.Default == null && description.DefaultValue != null)
            {
                var json = JsonSerializer.Serialize(description.DefaultValue);
                parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
            }

            parameter.Required |= description.IsRequired;
        }
    }
}

/// <summary>
/// Filter to exclude certain endpoints from Swagger documentation
/// </summary>
public class SwaggerExcludeFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Example: Exclude test endpoints in production
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment != "Development" && 
            context.ApiDescription.RelativePath?.Contains("test", StringComparison.OrdinalIgnoreCase) == true)
        {
            operation.Tags.Clear();
            operation.Tags.Add(new OpenApiTag { Name = "Internal" });
        }
    }
}

/// <summary>
/// Document filter for global Swagger document modifications
/// </summary>
public class SwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Add custom tags
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new() { Name = "Products", Description = "Product management operations" },
            new() { Name = "Orders", Description = "Order management operations" },
            new() { Name = "Customers", Description = "Customer management operations" },
            new() { Name = "Health", Description = "Health check endpoints" }
        };

        // Remove unwanted paths
        var pathsToRemove = swaggerDoc.Paths
            .Where(x => x.Key.Contains("test", StringComparison.OrdinalIgnoreCase) && 
                       Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
            .ToList();

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path.Key);
        }
    }
}

/// <summary>
/// Schema filter for better model documentation
/// </summary>
public class SwaggerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Add examples for common types
        if (context.Type == typeof(Guid) || context.Type == typeof(Guid?))
        {
            schema.Example = OpenApiAnyFactory.CreateFromJson("\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"");
        }

        if (context.Type == typeof(DateTime) || context.Type == typeof(DateTime?))
        {
            schema.Example = OpenApiAnyFactory.CreateFromJson("\"2024-01-15T10:30:00Z\"");
        }

        // Set required properties based on attributes
        if (schema.Properties != null)
        {
            var requiredProperties = context.Type.GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false).Any())
                .Select(p => char.ToLowerInvariant(p.Name[0]) + p.Name[1..])
                .ToList();

            foreach (var requiredProperty in requiredProperties)
            {
                if (schema.Properties.ContainsKey(requiredProperty))
                {
                    schema.Required.Add(requiredProperty);
                }
            }
        }
    }
}