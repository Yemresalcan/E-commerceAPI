# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY ECommerce.Solution.sln ./

# Copy project files
COPY src/Core/ECommerce.Domain/ECommerce.Domain.csproj src/Core/ECommerce.Domain/
COPY src/Core/ECommerce.Application/ECommerce.Application.csproj src/Core/ECommerce.Application/
COPY src/Infrastructure/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj src/Infrastructure/ECommerce.Infrastructure/
COPY src/Infrastructure/ECommerce.ReadModel/ECommerce.ReadModel.csproj src/Infrastructure/ECommerce.ReadModel/
COPY src/Presentation/ECommerce.WebAPI/ECommerce.WebAPI.csproj src/Presentation/ECommerce.WebAPI/

# Copy test project files
COPY tests/ECommerce.Domain.Tests/ECommerce.Domain.Tests.csproj tests/ECommerce.Domain.Tests/
COPY tests/ECommerce.Application.Tests/ECommerce.Application.Tests.csproj tests/ECommerce.Application.Tests/
COPY tests/ECommerce.Infrastructure.Tests/ECommerce.Infrastructure.Tests.csproj tests/ECommerce.Infrastructure.Tests/
COPY tests/ECommerce.WebAPI.Tests/ECommerce.WebAPI.Tests.csproj tests/ECommerce.WebAPI.Tests/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish src/Presentation/ECommerce.WebAPI/ECommerce.WebAPI.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 9 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy the published application
COPY --from=build /app/publish .

# Change ownership of the app directory to the appuser
RUN chown -R appuser:appuser /app

# Switch to the non-root user
USER appuser

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "ECommerce.WebAPI.dll"]