#!/bin/bash

# Production environment startup script
set -e

echo "Starting ECommerce API in production mode..."

# Check if .env file exists
if [ ! -f .env ]; then
    echo "Error: .env file not found. Please create it with production values."
    exit 1
fi

# Load environment variables
source .env

# Stop any existing containers
echo "Stopping existing containers..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# Pull latest images
echo "Pulling latest images..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml pull

# Build and start services
echo "Building and starting services..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d

# Wait for services to be healthy
echo "Waiting for services to be healthy..."
sleep 60

# Check service health
echo "Checking service health..."
docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps

# Run health check
echo "Running health check..."
curl -f http://localhost:${API_PORT}/health || echo "Health check failed"

echo "Production environment is ready!"
echo "API: http://localhost:${API_PORT}"
echo "Health: http://localhost:${API_PORT}/health"