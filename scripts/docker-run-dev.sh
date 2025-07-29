#!/bin/bash

# Development environment startup script
set -e

echo "Starting ECommerce API in development mode..."

# Stop any existing containers
echo "Stopping existing containers..."
docker-compose down

# Build and start services
echo "Building and starting services..."
docker-compose up --build -d

# Wait for services to be healthy
echo "Waiting for services to be healthy..."
sleep 30

# Check service health
echo "Checking service health..."
docker-compose ps

# Show logs
echo "Showing recent logs..."
docker-compose logs --tail=50

echo "Development environment is ready!"
echo "API: http://localhost:8080"
echo "Swagger: http://localhost:8080/swagger"
echo "Health: http://localhost:8080/health"
echo "RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "Elasticsearch: http://localhost:9200"