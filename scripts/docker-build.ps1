# Docker build script for ECommerce API
param(
    [string]$Environment = ""
)

Write-Host "Building ECommerce API Docker image..." -ForegroundColor Green

try {
    # Build the Docker image
    docker build -t ecommerce-api:latest .
    
    Write-Host "Docker image built successfully!" -ForegroundColor Green
    
    # Optional: Tag for different environments
    if ($Environment -eq "prod") {
        docker tag ecommerce-api:latest ecommerce-api:production
        Write-Host "Tagged as production image" -ForegroundColor Yellow
    }
    elseif ($Environment -eq "dev") {
        docker tag ecommerce-api:latest ecommerce-api:development
        Write-Host "Tagged as development image" -ForegroundColor Yellow
    }
    
    Write-Host "Available images:" -ForegroundColor Cyan
    docker images | Select-String "ecommerce-api"
}
catch {
    Write-Host "Error building Docker image: $_" -ForegroundColor Red
    exit 1
}