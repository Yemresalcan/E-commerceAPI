#!/bin/bash

# E-Commerce API Monitoring Setup Script
# This script sets up comprehensive monitoring for the E-Commerce API

set -e

echo "🚀 Setting up E-Commerce API Monitoring..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${BLUE}=== $1 ===${NC}"
}

# Check if Docker is running
check_docker() {
    print_header "Checking Docker"
    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    print_status "Docker is running ✓"
}

# Check if Docker Compose is available
check_docker_compose() {
    print_header "Checking Docker Compose"
    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        print_error "Docker Compose is not available. Please install Docker Compose."
        exit 1
    fi
    print_status "Docker Compose is available ✓"
}

# Create necessary directories
create_directories() {
    print_header "Creating Directories"
    
    directories=(
        "config/grafana/dashboards"
        "config/grafana/datasources"
        "logs"
        "data/prometheus"
        "data/grafana"
        "data/loki"
        "data/alertmanager"
    )
    
    for dir in "${directories[@]}"; do
        if [ ! -d "$dir" ]; then
            mkdir -p "$dir"
            print_status "Created directory: $dir"
        fi
    done
}

# Set proper permissions
set_permissions() {
    print_header "Setting Permissions"
    
    # Grafana needs specific user permissions
    sudo chown -R 472:472 data/grafana 2>/dev/null || true
    
    # Prometheus needs specific permissions
    sudo chown -R 65534:65534 data/prometheus 2>/dev/null || true
    
    # Loki needs write permissions
    sudo chown -R 10001:10001 data/loki 2>/dev/null || true
    
    print_status "Permissions set ✓"
}

# Start monitoring stack
start_monitoring() {
    print_header "Starting Monitoring Stack"
    
    # Start only monitoring services
    docker-compose -f docker-compose.monitoring.yml up -d
    
    print_status "Monitoring stack started ✓"
}

# Start full stack with monitoring
start_full_stack() {
    print_header "Starting Full Stack with Monitoring"
    
    # Start all services including the application
    docker-compose up -d
    
    print_status "Full stack started ✓"
}

# Wait for services to be ready
wait_for_services() {
    print_header "Waiting for Services"
    
    services=(
        "prometheus:9090"
        "grafana:3000"
        "alertmanager:9093"
        "loki:3100"
    )
    
    for service in "${services[@]}"; do
        IFS=':' read -r name port <<< "$service"
        print_status "Waiting for $name to be ready..."
        
        timeout=60
        while ! nc -z localhost "$port" 2>/dev/null; do
            sleep 2
            timeout=$((timeout - 2))
            if [ $timeout -le 0 ]; then
                print_warning "$name is not responding on port $port"
                break
            fi
        done
        
        if nc -z localhost "$port" 2>/dev/null; then
            print_status "$name is ready ✓"
        fi
    done
}

# Display access information
show_access_info() {
    print_header "Access Information"
    
    echo -e "${BLUE}Monitoring Services:${NC}"
    echo "📊 Grafana Dashboard:    http://localhost:3000 (admin/admin)"
    echo "📈 Prometheus:           http://localhost:9090"
    echo "🚨 AlertManager:         http://localhost:9093"
    echo "📋 Loki:                 http://localhost:3100"
    echo "📊 cAdvisor:             http://localhost:8081"
    echo "💻 Node Exporter:        http://localhost:9100"
    echo "⏰ Uptime Kuma:          http://localhost:3001"
    echo "🐳 Portainer:            http://localhost:9000"
    echo ""
    echo -e "${BLUE}Database Exporters:${NC}"
    echo "🐘 PostgreSQL Exporter:  http://localhost:9187"
    echo "🔴 Redis Exporter:       http://localhost:9121"
    echo "🔍 Elasticsearch Exp.:   http://localhost:9114"
    echo "🐰 RabbitMQ Exporter:    http://localhost:9419"
    echo ""
    echo -e "${BLUE}Application:${NC}"
    echo "🛒 E-Commerce API:       http://localhost:8080"
    echo "📖 API Documentation:    http://localhost:8080/swagger"
    echo "❤️  Health Checks:       http://localhost:8080/health"
}

# Import Grafana dashboards
import_dashboards() {
    print_header "Importing Grafana Dashboards"
    
    # Wait for Grafana to be fully ready
    sleep 10
    
    # Import the main dashboard
    if [ -f "config/grafana/dashboards/ecommerce-overview.json" ]; then
        curl -X POST \
            -H "Content-Type: application/json" \
            -d @config/grafana/dashboards/ecommerce-overview.json \
            http://admin:admin@localhost:3000/api/dashboards/db 2>/dev/null || true
        print_status "Dashboard imported ✓"
    fi
}

# Health check
health_check() {
    print_header "Health Check"
    
    # Check if all containers are running
    failed_containers=$(docker-compose ps --services --filter "status=exited")
    
    if [ -n "$failed_containers" ]; then
        print_warning "Some containers are not running:"
        echo "$failed_containers"
    else
        print_status "All containers are running ✓"
    fi
    
    # Check if metrics are being collected
    if curl -s http://localhost:9090/api/v1/targets | grep -q "up"; then
        print_status "Prometheus is collecting metrics ✓"
    else
        print_warning "Prometheus may not be collecting metrics properly"
    fi
}

# Main execution
main() {
    print_header "E-Commerce API Monitoring Setup"
    
    # Parse command line arguments
    MONITORING_ONLY=false
    SKIP_HEALTH_CHECK=false
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            --monitoring-only)
                MONITORING_ONLY=true
                shift
                ;;
            --skip-health-check)
                SKIP_HEALTH_CHECK=true
                shift
                ;;
            --help)
                echo "Usage: $0 [OPTIONS]"
                echo "Options:"
                echo "  --monitoring-only     Start only monitoring services"
                echo "  --skip-health-check   Skip health check at the end"
                echo "  --help               Show this help message"
                exit 0
                ;;
            *)
                print_error "Unknown option: $1"
                exit 1
                ;;
        esac
    done
    
    # Execute setup steps
    check_docker
    check_docker_compose
    create_directories
    set_permissions
    
    if [ "$MONITORING_ONLY" = true ]; then
        start_monitoring
    else
        start_full_stack
    fi
    
    wait_for_services
    import_dashboards
    
    if [ "$SKIP_HEALTH_CHECK" = false ]; then
        health_check
    fi
    
    show_access_info
    
    print_status "Setup completed successfully! 🎉"
    print_status "You can now access the monitoring dashboards using the URLs above."
}

# Run main function
main "$@"