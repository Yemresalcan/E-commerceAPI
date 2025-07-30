# ECommerce Solution Deployment Guide

## Overview

This guide provides comprehensive instructions for deploying the ECommerce solution across different environments, from local development to production deployment.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Environment Setup](#environment-setup)
- [Local Development Deployment](#local-development-deployment)
- [Staging Deployment](#staging-deployment)
- [Production Deployment](#production-deployment)
- [Database Migration](#database-migration)
- [Monitoring and Health Checks](#monitoring-and-health-checks)
- [Backup and Recovery](#backup-and-recovery)
- [Troubleshooting](#troubleshooting)
- [Security Considerations](#security-considerations)

## Prerequisites

### System Requirements

#### Minimum Requirements
- **CPU**: 2 cores
- **RAM**: 4GB
- **Storage**: 20GB available space
- **Network**: Stable internet connection

#### Recommended Requirements
- **CPU**: 4+ cores
- **RAM**: 8GB+
- **Storage**: 50GB+ SSD
- **Network**: High-speed internet connection

### Software Dependencies

#### Required Software
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (v4.0+)
- [Docker Compose](https://docs.docker.com/compose/) (v2.0+)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)

#### Optional Tools
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [pgAdmin](https://www.pgadmin.org/) for PostgreSQL management
- [Redis Insight](https://redis.com/redis-enterprise/redis-insight/) for Redis management
- [Postman](https://www.postman.com/) for API testing

### Platform Support

| Platform | Development | Production |
|----------|-------------|------------|
| Windows 10/11 | ✅ | ✅ |
| macOS | ✅ | ✅ |
| Linux (Ubuntu 20.04+) | ✅ | ✅ |
| Docker | ✅ | ✅ |
| Kubernetes | ❌ | ✅ |

## Environment Setup

### Environment Variables

Create environment-specific configuration files:

#### Development (.env.development)
```env
# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
API_PORT=8080

# Database Configuration
POSTGRES_DB=ecommerce_dev
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
CONNECTION_STRING=Host=localhost;Port=5432;Database=ecommerce_dev;Username=postgres;Password=postgres

# Redis Configuration
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=
REDIS_CONNECTION_STRING=localhost:6379

# Elasticsearch Configuration
ELASTICSEARCH_HOST=localhost
ELASTICSEARCH_PORT=9200
ELASTICSEARCH_URL=http://localhost:9200

# RabbitMQ Configuration
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
RABBITMQ_CONNECTION_STRING=amqp://guest:guest@localhost:5672/

# Logging Configuration
SERILOG_MINIMUM_LEVEL=Debug
SERILOG_WRITE_TO_CONSOLE=true
SERILOG_WRITE_TO_FILE=true

# Feature Flags
ENABLE_SWAGGER=true
ENABLE_HEALTH_CHECKS_UI=true
ENABLE_DETAILED_ERRORS=true
```

#### Staging (.env.staging)
```env
# Application Configuration
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://+:8080
API_PORT=8080

# Database Configuration
POSTGRES_DB=ecommerce_staging
POSTGRES_USER=ecommerce_user
POSTGRES_PASSWORD=staging_secure_password_123
POSTGRES_HOST=postgres-staging
POSTGRES_PORT=5432

# Redis Configuration
REDIS_HOST=redis-staging
REDIS_PORT=6379
REDIS_PASSWORD=staging_redis_password

# Elasticsearch Configuration
ELASTICSEARCH_HOST=elasticsearch-staging
ELASTICSEARCH_PORT=9200
ELASTICSEARCH_USERNAME=elastic
ELASTICSEARCH_PASSWORD=staging_elastic_password

# RabbitMQ Configuration
RABBITMQ_HOST=rabbitmq-staging
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=ecommerce_user
RABBITMQ_PASSWORD=staging_rabbitmq_password

# Logging Configuration
SERILOG_MINIMUM_LEVEL=Information
SERILOG_WRITE_TO_CONSOLE=true
SERILOG_WRITE_TO_FILE=true

# Feature Flags
ENABLE_SWAGGER=true
ENABLE_HEALTH_CHECKS_UI=true
ENABLE_DETAILED_ERRORS=false
```

#### Production (.env.production)
```env
# Application Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
API_PORT=8080

# Database Configuration
POSTGRES_DB=ecommerce_prod
POSTGRES_USER=ecommerce_user
POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
POSTGRES_HOST=postgres-prod
POSTGRES_PORT=5432

# Redis Configuration
REDIS_HOST=redis-prod
REDIS_PORT=6379
REDIS_PASSWORD=${REDIS_PASSWORD}

# Elasticsearch Configuration
ELASTICSEARCH_HOST=elasticsearch-prod
ELASTICSEARCH_PORT=9200
ELASTICSEARCH_USERNAME=elastic
ELASTICSEARCH_PASSWORD=${ELASTICSEARCH_PASSWORD}

# RabbitMQ Configuration
RABBITMQ_HOST=rabbitmq-prod
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=ecommerce_user
RABBITMQ_PASSWORD=${RABBITMQ_PASSWORD}

# Logging Configuration
SERILOG_MINIMUM_LEVEL=Warning
SERILOG_WRITE_TO_CONSOLE=false
SERILOG_WRITE_TO_FILE=true

# Feature Flags
ENABLE_SWAGGER=false
ENABLE_HEALTH_CHECKS_UI=false
ENABLE_DETAILED_ERRORS=false

# Security Configuration
JWT_SECRET_KEY=${JWT_SECRET_KEY}
ENCRYPTION_KEY=${ENCRYPTION_KEY}
```

## Local Development Deployment

### Quick Start

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd ECommerce.Solution
   ```

2. **Setup Environment**
   ```bash
   # Copy environment configuration
   cp .env.example .env
   
   # Or create from template
   cp .env.development .env
   ```

3. **Start Infrastructure Services**
   ```bash
   # Start all services
   docker-compose up -d
   
   # Or use the provided script
   # Linux/Mac/WSL:
   ./scripts/docker-run-dev.sh
   
   # Windows PowerShell:
   .\scripts\docker-run-dev.ps1
   ```

4. **Verify Services**
   ```bash
   # Check service status
   docker-compose ps
   
   # View logs
   docker-compose logs -f
   ```

5. **Run Database Migrations**
   ```bash
   # Apply migrations
   dotnet ef database update --project src/Infrastructure/ECommerce.Infrastructure
   ```

6. **Start the Application**
   ```bash
   # Run the API
   dotnet run --project src/Presentation/ECommerce.WebAPI
   
   # Or with hot reload
   dotnet watch run --project src/Presentation/ECommerce.WebAPI
   ```

### Development Services

| Service | URL | Credentials |
|---------|-----|-------------|
| API | http://localhost:8080 | N/A |
| Swagger UI | http://localhost:8080/swagger | N/A |
| Health Checks | http://localhost:8080/health | N/A |
| Health Checks UI | http://localhost:8080/healthchecks-ui | N/A |
| PostgreSQL | localhost:5432 | postgres/postgres |
| Redis | localhost:6379 | No password |
| Elasticsearch | http://localhost:9200 | No auth |
| RabbitMQ Management | http://localhost:15672 | guest/guest |

### Development Workflow

1. **Code Changes**
   - Modify source code
   - Tests run automatically (if configured)
   - Hot reload updates the application

2. **Database Changes**
   ```bash
   # Add migration
   dotnet ef migrations add MigrationName --project src/Infrastructure/ECommerce.Infrastructure
   
   # Update database
   dotnet ef database update --project src/Infrastructure/ECommerce.Infrastructure
   ```

3. **Testing**
   ```bash
   # Run all tests
   dotnet test
   
   # Run specific test project
   dotnet test tests/ECommerce.Domain.Tests/
   
   # Run with coverage
   dotnet test --collect:"XPlat Code Coverage"
   ```

4. **Debugging**
   - Use IDE debugging features
   - Check application logs: `docker-compose logs -f ecommerce-api`
   - Monitor health checks: http://localhost:8080/health

## Staging Deployment

### Infrastructure Setup

1. **Prepare Staging Environment**
   ```bash
   # Create staging directory
   mkdir ecommerce-staging
   cd ecommerce-staging
   
   # Clone repository
   git clone <repository-url> .
   
   # Checkout staging branch (if applicable)
   git checkout staging
   ```

2. **Configure Environment**
   ```bash
   # Copy staging configuration
   cp .env.staging .env
   
   # Update passwords and secrets
   nano .env
   ```

3. **Deploy Services**
   ```bash
   # Build and start staging environment
   docker-compose -f docker-compose.yml -f docker-compose.staging.yml up -d --build
   
   # Or use staging script
   ./scripts/docker-run-staging.sh
   ```

### Staging Configuration

#### docker-compose.staging.yml
```yaml
version: '3.8'

services:
  ecommerce-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Staging
    deploy:
      resources:
        limits:
          memory: 1G
          cpus: '1.0'
        reservations:
          memory: 512M
          cpus: '0.5'

  postgres:
    environment:
      - POSTGRES_DB=ecommerce_staging
      - POSTGRES_USER=ecommerce_user
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    volumes:
      - postgres_staging_data:/var/lib/postgresql/data
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '1.0'

  redis:
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis_staging_data:/data
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '0.5'

  elasticsearch:
    environment:
      - ELASTIC_PASSWORD=${ELASTICSEARCH_PASSWORD}
      - xpack.security.enabled=true
    volumes:
      - elasticsearch_staging_data:/usr/share/elasticsearch/data
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '1.0'

  rabbitmq:
    environment:
      - RABBITMQ_DEFAULT_USER=ecommerce_user
      - RABBITMQ_DEFAULT_PASS=${RABBITMQ_PASSWORD}
    volumes:
      - rabbitmq_staging_data:/var/lib/rabbitmq
    deploy:
      resources:
        limits:
          memory: 1G
          cpus: '0.5'

volumes:
  postgres_staging_data:
  redis_staging_data:
  elasticsearch_staging_data:
  rabbitmq_staging_data:

networks:
  default:
    name: ecommerce-staging-network
    driver: bridge
```

### Staging Verification

1. **Health Checks**
   ```bash
   # Check all services
   curl http://staging-server:8080/health
   
   # Check individual services
   docker-compose ps
   docker-compose logs ecommerce-api
   ```

2. **Smoke Tests**
   ```bash
   # Test API endpoints
   curl http://staging-server:8080/api/v1.0/products
   curl http://staging-server:8080/api/v1.0/customers
   curl http://staging-server:8080/api/v1.0/orders
   ```

3. **Performance Testing**
   ```bash
   # Install k6 (load testing tool)
   # Run performance tests
   k6 run performance-tests/load-test.js
   ```

## Production Deployment

### Pre-Deployment Checklist

- [ ] All tests passing
- [ ] Security review completed
- [ ] Performance testing completed
- [ ] Database migration scripts tested
- [ ] Backup procedures verified
- [ ] Monitoring and alerting configured
- [ ] SSL certificates installed
- [ ] Environment variables secured
- [ ] Resource limits configured
- [ ] Health checks configured

### Production Infrastructure

#### High Availability Setup

```bash
# Production deployment with multiple instances
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --scale ecommerce-api=3
```

#### docker-compose.prod.yml
```yaml
version: '3.8'

services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./nginx/ssl:/etc/nginx/ssl
    depends_on:
      - ecommerce-api
    networks:
      - ecommerce-prod-network

  ecommerce-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
    deploy:
      replicas: 3
      resources:
        limits:
          memory: 2G
          cpus: '2.0'
        reservations:
          memory: 1G
          cpus: '1.0'
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  postgres:
    environment:
      - POSTGRES_DB=ecommerce_prod
      - POSTGRES_USER=ecommerce_user
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    volumes:
      - postgres_prod_data:/var/lib/postgresql/data
      - ./backups:/backups
    deploy:
      resources:
        limits:
          memory: 4G
          cpus: '2.0'
        reservations:
          memory: 2G
          cpus: '1.0'
    command: >
      postgres
      -c max_connections=200
      -c shared_buffers=256MB
      -c effective_cache_size=1GB
      -c maintenance_work_mem=64MB
      -c checkpoint_completion_target=0.9
      -c wal_buffers=16MB
      -c default_statistics_target=100

  redis:
    command: >
      redis-server
      --requirepass ${REDIS_PASSWORD}
      --maxmemory 1gb
      --maxmemory-policy allkeys-lru
      --save 900 1
      --save 300 10
      --save 60 10000
    volumes:
      - redis_prod_data:/data
    deploy:
      resources:
        limits:
          memory: 1G
          cpus: '1.0'
        reservations:
          memory: 512M
          cpus: '0.5'

  elasticsearch:
    environment:
      - cluster.name=ecommerce-prod
      - node.name=es-node-1
      - discovery.type=single-node
      - ES_JAVA_OPTS=-Xms2g -Xmx2g
      - ELASTIC_PASSWORD=${ELASTICSEARCH_PASSWORD}
      - xpack.security.enabled=true
      - xpack.security.http.ssl.enabled=false
      - xpack.security.transport.ssl.enabled=false
    volumes:
      - elasticsearch_prod_data:/usr/share/elasticsearch/data
    deploy:
      resources:
        limits:
          memory: 4G
          cpus: '2.0'
        reservations:
          memory: 2G
          cpus: '1.0'

  rabbitmq:
    environment:
      - RABBITMQ_DEFAULT_USER=ecommerce_user
      - RABBITMQ_DEFAULT_PASS=${RABBITMQ_PASSWORD}
      - RABBITMQ_VM_MEMORY_HIGH_WATERMARK=0.8
    volumes:
      - rabbitmq_prod_data:/var/lib/rabbitmq
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '1.0'
        reservations:
          memory: 1G
          cpus: '0.5'

  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./monitoring/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--storage.tsdb.retention.time=200h'
      - '--web.enable-lifecycle'

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=${GRAFANA_PASSWORD}
    volumes:
      - grafana_data:/var/lib/grafana
      - ./monitoring/grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./monitoring/grafana/datasources:/etc/grafana/provisioning/datasources

volumes:
  postgres_prod_data:
  redis_prod_data:
  elasticsearch_prod_data:
  rabbitmq_prod_data:
  prometheus_data:
  grafana_data:

networks:
  ecommerce-prod-network:
    driver: bridge
    ipam:
      config:
        - subnet: 172.20.0.0/16
```

### Production Deployment Steps

1. **Prepare Production Server**
   ```bash
   # Update system packages
   sudo apt update && sudo apt upgrade -y
   
   # Install Docker and Docker Compose
   curl -fsSL https://get.docker.com -o get-docker.sh
   sudo sh get-docker.sh
   sudo usermod -aG docker $USER
   
   # Install Docker Compose
   sudo curl -L "https://github.com/docker/compose/releases/download/v2.20.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
   sudo chmod +x /usr/local/bin/docker-compose
   ```

2. **Deploy Application**
   ```bash
   # Clone repository
   git clone <repository-url> /opt/ecommerce
   cd /opt/ecommerce
   
   # Checkout production branch
   git checkout main
   
   # Set up environment
   cp .env.production .env
   
   # Update secrets (use proper secret management in production)
   nano .env
   
   # Deploy services
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
   ```

3. **Configure Reverse Proxy (Nginx)**
   ```nginx
   # /etc/nginx/sites-available/ecommerce
   upstream ecommerce_api {
       server 127.0.0.1:8080;
       server 127.0.0.1:8081;
       server 127.0.0.1:8082;
   }
   
   server {
       listen 80;
       server_name api.ecommerce.com;
       return 301 https://$server_name$request_uri;
   }
   
   server {
       listen 443 ssl http2;
       server_name api.ecommerce.com;
   
       ssl_certificate /etc/ssl/certs/ecommerce.crt;
       ssl_certificate_key /etc/ssl/private/ecommerce.key;
       ssl_protocols TLSv1.2 TLSv1.3;
       ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
       ssl_prefer_server_ciphers off;
   
       location / {
           proxy_pass http://ecommerce_api;
           proxy_set_header Host $host;
           proxy_set_header X-Real-IP $remote_addr;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
           
           # Health check
           proxy_connect_timeout 5s;
           proxy_send_timeout 60s;
           proxy_read_timeout 60s;
       }
   
       location /health {
           proxy_pass http://ecommerce_api/health;
           access_log off;
       }
   }
   ```

4. **Enable and Start Services**
   ```bash
   # Enable Nginx site
   sudo ln -s /etc/nginx/sites-available/ecommerce /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl reload nginx
   
   # Set up systemd service for Docker Compose
   sudo tee /etc/systemd/system/ecommerce.service > /dev/null <<EOF
   [Unit]
   Description=ECommerce Application
   Requires=docker.service
   After=docker.service
   
   [Service]
   Type=oneshot
   RemainAfterExit=yes
   WorkingDirectory=/opt/ecommerce
   ExecStart=/usr/local/bin/docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
   ExecStop=/usr/local/bin/docker-compose -f docker-compose.yml -f docker-compose.prod.yml down
   TimeoutStartSec=0
   
   [Install]
   WantedBy=multi-user.target
   EOF
   
   sudo systemctl enable ecommerce.service
   sudo systemctl start ecommerce.service
   ```

## Database Migration

### Migration Strategy

1. **Development Migrations**
   ```bash
   # Add new migration
   dotnet ef migrations add AddNewFeature --project src/Infrastructure/ECommerce.Infrastructure
   
   # Review generated migration
   # Apply migration
   dotnet ef database update --project src/Infrastructure/ECommerce.Infrastructure
   ```

2. **Production Migrations**
   ```bash
   # Generate SQL script for production
   dotnet ef migrations script --project src/Infrastructure/ECommerce.Infrastructure --output migration.sql
   
   # Review SQL script before applying
   # Apply in production with proper backup
   ```

### Migration Best Practices

1. **Backup Before Migration**
   ```bash
   # Create database backup
   docker-compose exec postgres pg_dump -U ecommerce_user ecommerce_prod > backup_$(date +%Y%m%d_%H%M%S).sql
   ```

2. **Test Migrations**
   ```bash
   # Test on staging environment first
   # Verify data integrity
   # Test rollback procedures
   ```

3. **Zero-Downtime Migrations**
   - Use backward-compatible changes
   - Deploy in multiple phases
   - Use feature flags for new functionality

## Monitoring and Health Checks

### Health Check Endpoints

| Endpoint | Description |
|----------|-------------|
| `/health` | Overall application health |
| `/health/ready` | Readiness probe |
| `/health/live` | Liveness probe |
| `/healthchecks-ui` | Health checks dashboard |

### Monitoring Setup

1. **Prometheus Configuration**
   ```yaml
   # monitoring/prometheus.yml
   global:
     scrape_interval: 15s
     evaluation_interval: 15s
   
   scrape_configs:
     - job_name: 'ecommerce-api'
       static_configs:
         - targets: ['ecommerce-api:8080']
       metrics_path: '/metrics'
       scrape_interval: 5s
   
     - job_name: 'postgres'
       static_configs:
         - targets: ['postgres:5432']
   
     - job_name: 'redis'
       static_configs:
         - targets: ['redis:6379']
   ```

2. **Grafana Dashboards**
   - Application performance metrics
   - Infrastructure monitoring
   - Business metrics
   - Error tracking

### Alerting Rules

```yaml
# monitoring/alert-rules.yml
groups:
  - name: ecommerce-alerts
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.1
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: High error rate detected
          
      - alert: DatabaseConnectionFailure
        expr: up{job="postgres"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: Database connection failure
```

## Backup and Recovery

### Automated Backup Script

```bash
#!/bin/bash
# scripts/backup.sh

BACKUP_DIR="/opt/ecommerce/backups"
DATE=$(date +%Y%m%d_%H%M%S)

# Create backup directory
mkdir -p $BACKUP_DIR

# Database backup
docker-compose exec -T postgres pg_dump -U ecommerce_user ecommerce_prod | gzip > $BACKUP_DIR/postgres_$DATE.sql.gz

# Redis backup
docker-compose exec -T redis redis-cli --rdb /data/dump.rdb
docker cp $(docker-compose ps -q redis):/data/dump.rdb $BACKUP_DIR/redis_$DATE.rdb

# Elasticsearch backup
curl -X PUT "localhost:9200/_snapshot/backup_repo/snapshot_$DATE?wait_for_completion=true"

# Clean old backups (keep last 7 days)
find $BACKUP_DIR -name "*.gz" -mtime +7 -delete
find $BACKUP_DIR -name "*.rdb" -mtime +7 -delete

echo "Backup completed: $DATE"
```

### Recovery Procedures

1. **Database Recovery**
   ```bash
   # Stop application
   docker-compose stop ecommerce-api
   
   # Restore database
   gunzip -c backup_20240115_120000.sql.gz | docker-compose exec -T postgres psql -U ecommerce_user ecommerce_prod
   
   # Start application
   docker-compose start ecommerce-api
   ```

2. **Full System Recovery**
   ```bash
   # Stop all services
   docker-compose down
   
   # Restore data volumes
   docker volume create postgres_prod_data
   docker run --rm -v postgres_prod_data:/data -v $(pwd)/backups:/backup alpine sh -c "cd /data && tar xzf /backup/postgres_data.tar.gz"
   
   # Start services
   docker-compose up -d
   ```

## Troubleshooting

### Common Issues

#### Service Won't Start
```bash
# Check service logs
docker-compose logs service-name

# Check resource usage
docker stats

# Check port conflicts
netstat -tulpn | grep :8080
```

#### Database Connection Issues
```bash
# Check PostgreSQL logs
docker-compose logs postgres

# Test connection
docker-compose exec postgres psql -U ecommerce_user -d ecommerce_prod -c "SELECT 1;"

# Check connection string
echo $CONNECTION_STRING
```

#### Performance Issues
```bash
# Monitor resource usage
docker stats

# Check application metrics
curl http://localhost:8080/metrics

# Analyze slow queries
docker-compose exec postgres psql -U ecommerce_user -d ecommerce_prod -c "SELECT * FROM pg_stat_activity WHERE state = 'active';"
```

### Log Analysis

```bash
# View application logs
docker-compose logs -f ecommerce-api

# Search for errors
docker-compose logs ecommerce-api | grep ERROR

# Export logs for analysis
docker-compose logs --no-color ecommerce-api > app.log
```

## Security Considerations

### Production Security Checklist

- [ ] Use strong, unique passwords for all services
- [ ] Enable SSL/TLS encryption
- [ ] Configure firewall rules
- [ ] Use secret management system
- [ ] Enable audit logging
- [ ] Regular security updates
- [ ] Network segmentation
- [ ] Access control and authentication
- [ ] Data encryption at rest
- [ ] Regular security scans

### Secret Management

```bash
# Use Docker secrets (Docker Swarm)
echo "super_secret_password" | docker secret create postgres_password -

# Use environment variable files
echo "POSTGRES_PASSWORD=super_secret_password" > .env.secrets
chmod 600 .env.secrets
```

### SSL Certificate Setup

```bash
# Generate self-signed certificate (development only)
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout nginx/ssl/ecommerce.key \
  -out nginx/ssl/ecommerce.crt

# For production, use Let's Encrypt
certbot --nginx -d api.ecommerce.com
```

## Maintenance

### Regular Maintenance Tasks

1. **Daily**
   - Monitor health checks
   - Review error logs
   - Check backup completion

2. **Weekly**
   - Update security patches
   - Review performance metrics
   - Clean up old logs and backups

3. **Monthly**
   - Security vulnerability scan
   - Performance optimization review
   - Disaster recovery testing

### Update Procedures

```bash
# Update application
git pull origin main
docker-compose build ecommerce-api
docker-compose up -d ecommerce-api

# Update infrastructure services
docker-compose pull
docker-compose up -d
```

This deployment guide provides comprehensive instructions for deploying the ECommerce solution across different environments while maintaining security, performance, and reliability standards.