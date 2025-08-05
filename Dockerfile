# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Presentation/ECommerce.WebAPI/ECommerce.WebAPI.csproj", "src/Presentation/ECommerce.WebAPI/"]
COPY ["src/Core/ECommerce.Application/ECommerce.Application.csproj", "src/Core/ECommerce.Application/"]
COPY ["src/Core/ECommerce.Domain/ECommerce.Domain.csproj", "src/Core/ECommerce.Domain/"]
COPY ["src/Infrastructure/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj", "src/Infrastructure/ECommerce.Infrastructure/"]
COPY ["src/Infrastructure/ECommerce.ReadModel/ECommerce.ReadModel.csproj", "src/Infrastructure/ECommerce.ReadModel/"]

RUN dotnet restore "src/Presentation/ECommerce.WebAPI/ECommerce.WebAPI.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/Presentation/ECommerce.WebAPI"
RUN dotnet build "ECommerce.WebAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ECommerce.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN addgroup --system --gid 1001 dotnet && \
    adduser --system --uid 1001 --gid 1001 --shell /bin/false dotnet

# Copy published application
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chown -R dotnet:dotnet /app

# Switch to non-root user
USER dotnet

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "ECommerce.WebAPI.dll"]