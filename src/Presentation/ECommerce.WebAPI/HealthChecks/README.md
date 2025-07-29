# Health Checks Documentation

This document describes the health check implementation for the ECommerce API.

## Overview

The application includes comprehensive health checks for all critical dependencies and services:

- **PostgreSQL Database** - Primary write database connectivity
- **Redis Cache** - Caching service connectivity
- **Elasticsearch** - Search and read model service
- **RabbitMQ** - Message queue service
- **Application Services** - Query services health
- **System Resources** - Memory, CPU, disk space monitoring

## Health Check Endpoints

### Basic Health Check
- **URL**: `/health`
- **Method**: GET
- **Description**: Simple health check returning HTTP 200 (healthy) or 503 (unhealthy)

### Detailed Health Check
- **URL**: `/health/detailed`
- **Method**: GET
- **Description**: Detailed JSON response with status of all components
- **Response Format**:
```json
{
  "status": "Healthy",
  "totalDuration": 125.5,
  "checks": [
    {
      "name": "postgresql",
      "status": "Healthy",
      "duration": 45.2,
      "description": "PostgreSQL database is healthy",
      "data": {},
      "tags": ["database", "postgresql"]
    }
  ]
}
```

### Readiness Probe
- **URL**: `/health/ready`
- **Method**: GET
- **Description**: Kubernetes readiness probe endpoint
- **Purpose**: Indicates if the application is ready to receive traffic

### Liveness Probe
- **URL**: `/health/live`
- **Method**: GET
- **Description**: Kubernetes liveness probe endpoint
- **Purpose**: Indicates if the application is alive and should not be restarted

### Health Check UI
- **URL**: `/health-ui`
- **Method**: GET
- **Description**: Web-based health check dashboard
- **Features**:
  - Real-time health status monitoring
  - Historical health data
  - Visual indicators for each component
  - Automatic refresh every 30 seconds

### API Endpoints

#### Get Overall Health
- **URL**: `/api/health`
- **Method**: GET
- **Description**: Programmatic access to health status

#### Get Component Health
- **URL**: `/api/health/{component}`
- **Method**: GET
- **Description**: Get health status of a specific component
- **Parameters**:
  - `component`: Component name (e.g., "postgresql", "redis", "elasticsearch")

#### Get Available Components
- **URL**: `/api/health/components`
- **Method**: GET
- **Description**: List all available health check components

## Health Check Components

### PostgreSQL Health Check
- **Name**: `postgresql`
- **Tags**: `database`, `postgresql`
- **Checks**: Database connectivity and query execution
- **Failure Conditions**: Connection timeout, authentication failure, query errors

### Redis Health Check
- **Name**: `redis`
- **Tags**: `cache`, `redis`
- **Checks**: Redis connectivity and basic operations
- **Failure Conditions**: Connection timeout, authentication failure, command errors

### Elasticsearch Health Check
- **Name**: `elasticsearch`
- **Tags**: `search`, `elasticsearch`
- **Checks**: 
  - Elasticsearch connectivity (ping)
  - Cluster health status
  - Node availability
- **Health States**:
  - **Green**: All shards allocated, cluster fully operational
  - **Yellow**: All primary shards allocated, some replicas missing (degraded)
  - **Red**: Some primary shards not allocated (unhealthy)

### RabbitMQ Health Check
- **Name**: `rabbitmq`
- **Tags**: `messaging`, `rabbitmq`
- **Checks**: RabbitMQ connectivity and broker status
- **Failure Conditions**: Connection timeout, authentication failure, broker errors

### Application Services Health Check
- **Name**: `application`
- **Tags**: `application`, `services`
- **Checks**: 
  - Product query service functionality
  - Order query service functionality
  - Customer query service functionality
- **Failure Conditions**: Service initialization errors, dependency failures

### System Resources Health Check
- **Name**: `system-resources`
- **Tags**: `system`, `resources`
- **Checks**:
  - Memory usage (working set and GC memory)
  - CPU usage percentage
  - Disk space availability
  - Thread pool utilization
- **Thresholds**:
  - Memory: 1 GB warning threshold
  - CPU: 80% warning threshold
  - Disk: 1 GB free space warning threshold
  - Thread Pool: 80% utilization warning threshold

## Configuration

Health checks can be configured in `appsettings.json`:

```json
{
  "HealthChecks": {
    "EvaluationTimeInSeconds": 30,
    "MaximumHistoryEntriesPerEndpoint": 50,
    "Endpoints": [
      {
        "Name": "ECommerce API",
        "Uri": "/health/detailed"
      }
    ]
  }
}
```

## Monitoring Integration

### Kubernetes
The health check endpoints are designed to work with Kubernetes health probes:

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 80
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 80
  initialDelaySeconds: 5
  periodSeconds: 5
```

### External Monitoring
External monitoring systems can use the following endpoints:
- `/health` - Simple up/down status
- `/health/detailed` - Detailed component status
- `/api/health` - Programmatic access with JSON response

## Troubleshooting

### Common Issues

1. **Database Connection Failures**
   - Check connection string configuration
   - Verify database server availability
   - Check network connectivity and firewall rules

2. **Elasticsearch Yellow Status**
   - Normal for single-node clusters
   - Consider adding replica nodes for production
   - Check index settings and shard allocation

3. **High Resource Usage**
   - Monitor memory leaks in application code
   - Check for inefficient queries or operations
   - Consider scaling resources or optimizing code

4. **Service Unavailable Responses**
   - Check application logs for detailed error information
   - Verify all dependencies are running and accessible
   - Check configuration settings and environment variables

### Logging

Health check activities are logged with structured logging:
- Debug level: Routine health check operations
- Warning level: Degraded component status
- Error level: Failed health checks and exceptions

Use the following log query to monitor health check issues:
```
SourceContext:"*HealthCheck*" AND Level:("Warning" OR "Error")
```