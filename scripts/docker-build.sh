#!/bin/bash

# Docker build script for ECommerce API
set -e

echo "Building ECommerce API Docker image..."

# Build the Docker image
docker build -t ecommerce-api:latest .

echo "Docker image built successfully!"

# Optional: Tag for different environments
if [ "$1" = "prod" ]; then
    docker tag ecommerce-api:latest ecommerce-api:production
    echo "Tagged as production image"
elif [ "$1" = "dev" ]; then
    docker tag ecommerce-api:latest ecommerce-api:development
    echo "Tagged as development image"
fi

echo "Available images:"
docker images | grep ecommerce-api