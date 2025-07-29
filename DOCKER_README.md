# Docker Configuration for ECommerce API

This document provides comprehensive instructions for running the ECommerce API using Docker and Docker Compose.

## Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose v2.0 or higher
- At least 4GB of available RAM
- At least 10GB of available disk space

## Quick Start

### Development Environment

1. **Clone the repository and navigate to the project root**
2. **Start the development environment:**

```bash
# Using bash script (Linux/Mac/WSL)
./scripts/docker-run-dev.sh

# Using PowerShell script (Windows)
.\scripts\docker-run-dev.ps1

# Or manually with docker-compose
docker-compose up --build -d
```

3. **Access the services:**
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger
   - Health Checks: http://localhost:8080/health
   - RabbitMQ Management: http://localhost:15672 (guest/guest)
   - Elasticsearch: http://localhost:9200

### Production Environment

1. **Configure environment variables:**
   ```bash
   cp .env.example .env
   # Edit .env with your production values
   ```

2. **Start the production environment:**
   ```bash
   # Using bash script (Linux/Mac/WSL)
   ./scripts/docker-run-prod.sh

   # Using PowerShell script (Windows)
   .\scripts\docker-run-prod.ps1

   # Or manually with docker-compose
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
   ```

## Services Overview

| Service | Port | Description |
|---------|------|-------------|
| ECommerce API | 8080 | Main web API application |
| PostgreSQL | 5432 | Primary database for write operations |
| Redis | 6379 | Caching and session storage |
| Elasticsearch | 9200, 9300 | Search engine for read operations |
| RabbitMQ | 5672, 15672 | Message broker for event handling |

## Configuration Files

### Docker Files

- **`Dockerfile`**: Multi-stage build configuration for the API
- **`docker-compose.yml`**: Base services configuration
- **`docker-compose.override.yml`**: Development overrides (auto-loaded)
- **`docker-compose.prod.yml`**: Production configuration
- **`.dockerignore`**: Files to exclude from Docker build context

### Environment Configuration

- **`.env`**: Environment variables for production
- **`rabbitmq/rabbitmq.conf`**: RabbitMQ server configuration
- **`rabbitmq/definitions.json`**: RabbitMQ queues, exchanges, and bindings
- **`scripts/init-db.sql`**: Database initialization script

## Environment Variables

### Database Configuration
```env
POSTGRES_DB=ecommerce
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
```

### Redis Configuration
```env
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_PASSWORD=
```

### Elasticsearch Configuration
```env
ELASTICSEARCH_HOST=elasticsearch
ELASTICSEARCH_PORT=9200
```

### RabbitMQ Configuration
```env
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
```

### Application Configuration
```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
API_PORT=8080
JWT_SECRET_KEY=your-super-secret-jwt-key-here
```

## Build Scripts

### Build Docker Image
```bash
# Linux/Mac/WSL
./scripts/docker-build.sh [dev|prod]

# Windows PowerShell
.\scripts\docker-build.ps1 -Environment [dev|prod]

# Manual build
docker build -t ecommerce-api:latest .
```

## Health Checks

All services include health checks that verify:

- **API**: HTTP endpoint responds correctly
- **PostgreSQL**: Database accepts connections
- **Redis**: Redis server responds to ping
- **Elasticsearch**: Cluster health is accessible
- **RabbitMQ**: Management API responds

Health check status can be viewed with:
```bash
docker-compose ps
```

## Volumes and Data Persistence

### Development
- `postgres_data`: PostgreSQL data
- `redis_data`: Redis data
- `elasticsearch_data`: Elasticsearch indices
- `rabbitmq_data`: RabbitMQ data
- `./logs`: Application logs (mounted from host)

### Production
- `postgres_prod_data`: PostgreSQL data
- `redis_prod_data`: Redis data
- `elasticsearch_prod_data`: Elasticsearch indices
- `rabbitmq_prod_data`: RabbitMQ data
- `./logs`: Application logs
- `./backups`: Database backups

## Networking

Services communicate through a custom bridge network (`ecommerce-network`) with the following benefits:
- Service discovery by name
- Isolated network environment
- Configurable IP ranges (production: 172.20.0.0/16)

## Resource Limits (Production)

| Service | Memory Limit | CPU Limit | Memory Reservation | CPU Reservation |
|---------|--------------|-----------|-------------------|-----------------|
| API | 1GB | 1.0 | 512MB | 0.5 |
| PostgreSQL | 2GB | 1.0 | 1GB | 0.5 |
| Redis | 512MB | 0.5 | 256MB | 0.25 |
| Elasticsearch | 2GB | 1.0 | 1GB | 0.5 |
| RabbitMQ | 1GB | 0.5 | 512MB | 0.25 |

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure ports 5432, 6379, 8080, 9200, 5672, 15672 are available
2. **Memory issues**: Ensure Docker has at least 4GB RAM allocated
3. **Build failures**: Clear Docker cache with `docker system prune -a`
4. **Service startup**: Check logs with `docker-compose logs [service-name]`

### Useful Commands

```bash
# View logs for all services
docker-compose logs -f

# View logs for specific service
docker-compose logs -f ecommerce-api

# Restart a specific service
docker-compose restart ecommerce-api

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Check service health
docker-compose ps

# Execute command in running container
docker-compose exec ecommerce-api bash

# View resource usage
docker stats
```

### Database Management

```bash
# Connect to PostgreSQL
docker-compose exec postgres psql -U postgres -d ecommerce

# Create database backup
docker-compose exec postgres pg_dump -U postgres ecommerce > backup.sql

# Restore database backup
docker-compose exec -T postgres psql -U postgres ecommerce < backup.sql
```

### Cache Management

```bash
# Connect to Redis
docker-compose exec redis redis-cli

# Clear Redis cache
docker-compose exec redis redis-cli FLUSHALL
```

### Search Index Management

```bash
# Check Elasticsearch health
curl http://localhost:9200/_cluster/health

# List indices
curl http://localhost:9200/_cat/indices

# Delete index
curl -X DELETE http://localhost:9200/ecommerce
```

## Security Considerations

### Development
- Default passwords are used for convenience
- Services are exposed on host ports for debugging
- Debug logging is enabled

### Production
- Change all default passwords
- Use environment variables for secrets
- Consider using Docker secrets for sensitive data
- Implement proper firewall rules
- Use HTTPS with reverse proxy (nginx/traefik)
- Regular security updates for base images

## Monitoring and Logging

### Application Logs
- Console output for development
- File logging with rotation
- Elasticsearch integration for centralized logging
- Structured logging with Serilog

### Health Monitoring
- Built-in health checks for all services
- Health check UI available at `/healthchecks-ui`
- Prometheus metrics endpoint (if configured)

## Scaling Considerations

### Horizontal Scaling
- API can be scaled with load balancer
- PostgreSQL requires read replicas for scaling reads
- Redis can use clustering for high availability
- Elasticsearch supports multi-node clusters
- RabbitMQ supports clustering and federation

### Vertical Scaling
- Adjust resource limits in docker-compose.prod.yml
- Monitor resource usage with `docker stats`
- Consider SSD storage for databases

## Backup and Recovery

### Automated Backups
```bash
# Database backup script (add to cron)
docker-compose exec postgres pg_dump -U postgres ecommerce | gzip > "backup_$(date +%Y%m%d_%H%M%S).sql.gz"
```

### Recovery Process
1. Stop the application
2. Restore database from backup
3. Clear Redis cache
4. Reindex Elasticsearch
5. Restart services

## Development Workflow

1. **Code changes**: Modify source code
2. **Rebuild**: `docker-compose up --build -d ecommerce-api`
3. **Test**: Run tests against containerized services
4. **Debug**: Use logs and health checks to troubleshoot
5. **Reset**: Use `docker-compose down -v` to reset all data

## Production Deployment

1. **Prepare environment**: Configure .env file
2. **Build images**: Use production Dockerfile
3. **Deploy**: Use docker-compose.prod.yml
4. **Monitor**: Check health endpoints and logs
5. **Backup**: Implement regular backup procedures
6. **Update**: Use blue-green deployment strategy

## Support

For issues related to Docker configuration:
1. Check service logs: `docker-compose logs [service]`
2. Verify health checks: `docker-compose ps`
3. Check resource usage: `docker stats`
4. Review configuration files
5. Consult Docker documentation