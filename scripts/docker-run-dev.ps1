# Development environment startup script

Write-Host "Starting ECommerce API in development mode..." -ForegroundColor Green

try {
    # Stop any existing containers
    Write-Host "Stopping existing containers..." -ForegroundColor Yellow
    docker-compose down
    
    # Build and start services
    Write-Host "Building and starting services..." -ForegroundColor Yellow
    docker-compose up --build -d
    
    # Wait for services to be healthy
    Write-Host "Waiting for services to be healthy..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    
    # Check service health
    Write-Host "Checking service health..." -ForegroundColor Yellow
    docker-compose ps
    
    # Show logs
    Write-Host "Showing recent logs..." -ForegroundColor Yellow
    docker-compose logs --tail=50
    
    Write-Host "Development environment is ready!" -ForegroundColor Green
    Write-Host "API: http://localhost:8080" -ForegroundColor Cyan
    Write-Host "Swagger: http://localhost:8080/swagger" -ForegroundColor Cyan
    Write-Host "Health: http://localhost:8080/health" -ForegroundColor Cyan
    Write-Host "RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor Cyan
    Write-Host "Elasticsearch: http://localhost:9200" -ForegroundColor Cyan
}
catch {
    Write-Host "Error starting development environment: $_" -ForegroundColor Red
    exit 1
}