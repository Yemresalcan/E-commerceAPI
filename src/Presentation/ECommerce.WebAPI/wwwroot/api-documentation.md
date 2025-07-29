# ECommerce API Documentation

## Overview

The ECommerce API is a comprehensive .NET 9 e-commerce solution built using Domain Driven Design (DDD), Command Query Responsibility Segregation (CQRS), and Clean Architecture patterns. This API provides a scalable, maintainable platform for managing products, orders, and customers.

## Architecture

The API follows Clean Architecture principles with clear separation of concerns:

- **Domain Layer**: Contains business logic, entities, and domain events
- **Application Layer**: Contains use cases, commands, queries, and handlers
- **Infrastructure Layer**: Contains data access, external services, and messaging
- **Presentation Layer**: Contains API controllers and HTTP concerns

## Authentication

The API supports multiple authentication methods:

### Bearer Token Authentication
```http
Authorization: Bearer <your-jwt-token>
```

### API Key Authentication
```http
X-Api-Key: <your-api-key>
```

## API Versioning

The API uses URL path versioning:
- Current version: `v1.0`
- Base URL pattern: `/api/v{version}/{controller}`
- Example: `/api/v1.0/products`

## Rate Limiting

The API implements rate limiting to ensure fair usage:
- **Anonymous users**: 100 requests per hour
- **Authenticated users**: 1000 requests per hour
- **Premium users**: 5000 requests per hour

## Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "data": { ... },
  "success": true,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Error Response
```json
{
  "error": {
    "message": "Error description",
    "code": "ERROR_CODE",
    "details": { ... }
  },
  "success": false,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Pagination

List endpoints support pagination with the following parameters:
- `page`: Page number (1-based, default: 1)
- `pageSize`: Items per page (1-100, default: 20)

### Pagination Response
```json
{
  "items": [...],
  "totalItems": 150,
  "totalPages": 8,
  "currentPage": 1,
  "pageSize": 20,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## Filtering and Sorting

Most list endpoints support filtering and sorting:

### Common Filter Parameters
- `searchTerm`: Text search across relevant fields
- `startDate` / `endDate`: Date range filtering
- `sortBy`: Sort field and direction

### Example
```http
GET /api/v1.0/products?searchTerm=laptop&minPrice=500&sortBy=price_asc&page=1&pageSize=10
```

## Error Codes

| Code | Description |
|------|-------------|
| `VALIDATION_ERROR` | Request validation failed |
| `NOT_FOUND` | Resource not found |
| `UNAUTHORIZED` | Authentication required |
| `FORBIDDEN` | Insufficient permissions |
| `RATE_LIMIT_EXCEEDED` | Too many requests |
| `INTERNAL_ERROR` | Server error |

## Health Checks

The API provides comprehensive health checks:

### Endpoints
- `GET /api/v1.0/health` - Overall health status
- `GET /api/v1.0/health/{component}` - Component-specific health
- `GET /api/v1.0/health/components` - Available components

### Health Status Values
- `Healthy`: All systems operational
- `Degraded`: Some non-critical issues
- `Unhealthy`: Critical issues detected

## Webhooks

The API supports webhooks for real-time notifications:

### Supported Events
- `product.created`
- `product.updated`
- `order.placed`
- `order.status_changed`
- `customer.registered`

### Webhook Payload
```json
{
  "eventType": "order.placed",
  "eventId": "uuid",
  "timestamp": "2024-01-15T10:30:00Z",
  "data": { ... }
}
```

## SDKs and Libraries

Official SDKs are available for:
- .NET/C#
- JavaScript/TypeScript
- Python
- Java

## Support

For API support and questions:
- Email: api-support@ecommerce.com
- Documentation: https://docs.ecommerce-api.com
- Status Page: https://status.ecommerce-api.com

## Changelog

### v1.0 (Current)
- Initial release
- Products, Orders, and Customers management
- Comprehensive search and filtering
- Health checks and monitoring
- Rate limiting and authentication