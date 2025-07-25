using ECommerce.Infrastructure.Persistence;
using ECommerce.ReadModel;
using ECommerce.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Add database services
builder.Services.AddDatabase(builder.Configuration);

// Configure health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created and migrations are applied
await DatabaseConfiguration.EnsureDatabaseCreatedAsync(app.Services);

// Ensure Elasticsearch indices are created
await app.Services.EnsureIndicesCreatedAsync();

app.Run();