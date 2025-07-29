# Production environment startup script

Write-Host "Starting ECommerce API in production mode..." -ForegroundColor Green

try {
    # Check if .env file exists
    if (-not (Test-Path ".env")) {
        Write-Host "Error: .env file not found. Please create it with production values." -ForegroundColor Red
        exit 1
    }
    
    # Load environment variables from .env file
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "^([^#][^=]+)=(.*)$") {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
        }
    }
    
    $API_PORT = $env:API_PORT
    if (-not $API_PORT) { $API_PORT = "8080" }
    
    # Stop any existing containers
    Write-Host "Stopping existing containers..." -ForegroundColor Yellow
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml down
    
    # Pull latest images
    Write-Host "Pulling latest images..." -ForegroundColor Yellow
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml pull
    
    # Build and start services
    Write-Host "Building and starting services..." -ForegroundColor Yellow
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
    
    # Wait for services to be healthy
    Write-Host "Waiting for services to be healthy..." -ForegroundColor Yellow
    Start-Sleep -Seconds 60
    
    # Check service health
    Write-Host "Checking service health..." -ForegroundColor Yellow
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps
    
    # Run health check
    Write-Host "Running health check..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$API_PORT/health" -UseBasicParsing
        Write-Host "Health check passed!" -ForegroundColor Green
    }
    catch {
        Write-Host "Health check failed: $_" -ForegroundColor Red
    }
    
    Write-Host "Production environment is ready!" -ForegroundColor Green
    Write-Host "API: http://localhost:$API_PORT" -ForegroundColor Cyan
    Write-Host "Health: http://localhost:$API_PORT/health" -ForegroundColor Cyan
}
catch {
    Write-Host "Error starting production environment: $_" -ForegroundColor Red
    exit 1
}