# E-Commerce API Monitoring Setup Script (PowerShell)
# This script sets up comprehensive monitoring for the E-Commerce API

param(
    [switch]$MonitoringOnly,
    [switch]$SkipHealthCheck,
    [switch]$Help
)

# Colors for output
$Red = "Red"
$Green = "Green"
$Yellow = "Yellow"
$Blue = "Blue"

function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Red
}

function Write-Header {
    param([string]$Message)
    Write-Host "=== $Message ===" -ForegroundColor $Blue
}

function Show-Help {
    Write-Host "Usage: .\scripts\monitoring-setup.ps1 [OPTIONS]"
    Write-Host "Options:"
    Write-Host "  -MonitoringOnly     Start only monitoring services"
    Write-Host "  -SkipHealthCheck    Skip health check at the end"
    Write-Host "  -Help              Show this help message"
    exit 0
}

function Test-Docker {
    Write-Header "Checking Docker"
    try {
        docker info | Out-Null
        Write-Status "Docker is running ✓"
    }
    catch {
        Write-Error "Docker is not running. Please start Docker and try again."
        exit 1
    }
}

function Test-DockerCompose {
    Write-Header "Checking Docker Compose"
    try {
        docker-compose --version | Out-Null
        Write-Status "Docker Compose is available ✓"
    }
    catch {
        try {
            docker compose version | Out-Null
            Write-Status "Docker Compose is available ✓"
        }
        catch {
            Write-Error "Docker Compose is not available. Please install Docker Compose."
            exit 1
        }
    }
}

function New-Directories {
    Write-Header "Creating Directories"
    
    $directories = @(
        "config\grafana\dashboards",
        "config\grafana\datasources",
        "logs",
        "data\prometheus",
        "data\grafana",
        "data\loki",
        "data\alertmanager"
    )
    
    foreach ($dir in $directories) {
        if (!(Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Status "Created directory: $dir"
        }
    }
}

function Start-MonitoringStack {
    Write-Header "Starting Monitoring Stack"
    
    if ($MonitoringOnly) {
        docker-compose -f docker-compose.monitoring.yml up -d
    }
    else {
        docker-compose up -d
    }
    
    Write-Status "Stack started ✓"
}

function Wait-ForServices {
    Write-Header "Waiting for Services"
    
    $services = @{
        "prometheus" = 9090
        "grafana" = 3000
        "alertmanager" = 9093
        "loki" = 3100
    }
    
    foreach ($service in $services.GetEnumerator()) {
        Write-Status "Waiting for $($service.Key) to be ready..."
        
        $timeout = 60
        do {
            Start-Sleep -Seconds 2
            $timeout -= 2
            try {
                $connection = Test-NetConnection -ComputerName localhost -Port $service.Value -WarningAction SilentlyContinue
                if ($connection.TcpTestSucceeded) {
                    Write-Status "$($service.Key) is ready ✓"
                    break
                }
            }
            catch {
                # Continue waiting
            }
        } while ($timeout -gt 0)
        
        if ($timeout -le 0) {
            Write-Warning "$($service.Key) is not responding on port $($service.Value)"
        }
    }
}

function Show-AccessInfo {
    Write-Header "Access Information"
    
    Write-Host "Monitoring Services:" -ForegroundColor $Blue
    Write-Host "📊 Grafana Dashboard:    http://localhost:3000 (admin/admin)"
    Write-Host "📈 Prometheus:           http://localhost:9090"
    Write-Host "🚨 AlertManager:         http://localhost:9093"
    Write-Host "📋 Loki:                 http://localhost:3100"
    Write-Host "📊 cAdvisor:             http://localhost:8081"
    Write-Host "💻 Node Exporter:        http://localhost:9100"
    Write-Host "⏰ Uptime Kuma:          http://localhost:3001"
    Write-Host "🐳 Portainer:            http://localhost:9000"
    Write-Host ""
    Write-Host "Database Exporters:" -ForegroundColor $Blue
    Write-Host "🐘 PostgreSQL Exporter:  http://localhost:9187"
    Write-Host "🔴 Redis Exporter:       http://localhost:9121"
    Write-Host "🔍 Elasticsearch Exp.:   http://localhost:9114"
    Write-Host "🐰 RabbitMQ Exporter:    http://localhost:9419"
    Write-Host ""
    Write-Host "Application:" -ForegroundColor $Blue
    Write-Host "🛒 E-Commerce API:       http://localhost:8080"
    Write-Host "📖 API Documentation:    http://localhost:8080/swagger"
    Write-Host "❤️  Health Checks:       http://localhost:8080/health"
}

function Import-GrafanaDashboards {
    Write-Header "Importing Grafana Dashboards"
    
    # Wait for Grafana to be fully ready
    Start-Sleep -Seconds 10
    
    # Import the main dashboard
    if (Test-Path "config\grafana\dashboards\ecommerce-overview.json") {
        try {
            $dashboard = Get-Content "config\grafana\dashboards\ecommerce-overview.json" -Raw
            Invoke-RestMethod -Uri "http://localhost:3000/api/dashboards/db" `
                -Method POST `
                -Headers @{"Content-Type" = "application/json"} `
                -Body $dashboard `
                -Credential (New-Object System.Management.Automation.PSCredential("admin", (ConvertTo-SecureString "admin" -AsPlainText -Force))) `
                -ErrorAction SilentlyContinue | Out-Null
            Write-Status "Dashboard imported ✓"
        }
        catch {
            Write-Warning "Could not import dashboard automatically"
        }
    }
}

function Test-Health {
    Write-Header "Health Check"
    
    # Check if containers are running
    try {
        $containers = docker-compose ps --services --filter "status=exited"
        if ($containers) {
            Write-Warning "Some containers are not running:"
            Write-Host $containers
        }
        else {
            Write-Status "All containers are running ✓"
        }
    }
    catch {
        Write-Warning "Could not check container status"
    }
    
    # Check if Prometheus is collecting metrics
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:9090/api/v1/targets" -ErrorAction SilentlyContinue
        if ($response -match "up") {
            Write-Status "Prometheus is collecting metrics ✓"
        }
        else {
            Write-Warning "Prometheus may not be collecting metrics properly"
        }
    }
    catch {
        Write-Warning "Could not check Prometheus metrics"
    }
}

# Main execution
function Main {
    if ($Help) {
        Show-Help
    }
    
    Write-Header "E-Commerce API Monitoring Setup"
    
    Test-Docker
    Test-DockerCompose
    New-Directories
    Start-MonitoringStack
    Wait-ForServices
    Import-GrafanaDashboards
    
    if (!$SkipHealthCheck) {
        Test-Health
    }
    
    Show-AccessInfo
    
    Write-Status "Setup completed successfully! 🎉"
    Write-Status "You can now access the monitoring dashboards using the URLs above."
}

# Run main function
Main