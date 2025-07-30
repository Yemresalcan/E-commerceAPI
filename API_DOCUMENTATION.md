# ECommerce API Documentation

## Overview

The ECommerce API is a RESTful web service built with .NET 9 that provides comprehensive e-commerce functionality. The API follows CQRS (Command Query Responsibility Segregation) patterns and implements Domain Driven Design principles.

## Base Information

- **Base URL**: `http://localhost:8080` (Development)
- **API Version**: `v1.0`
- **Content Type**: `application/json`
- **API Prefix**: `/api/v1.0`

## Authentication

Currently, the API does not implement authentication. For production deployment, consider implementing:
- JWT Bearer tokens
- OAuth 2.0 / OpenID Connect
- API Key authentication

## Common Response Patterns

### Success Responses

#### Single Resource
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Product Name",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### Collection with Pagination
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "Product Name"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Error Responses

#### Validation Error (400 Bad Request)
```json
{
  "error": "Validation failed",
  "details": [
    {
      "field": "email",
      "message": "Email is required"
    },
    {
      "field": "price",
      "message": "Price must be greater than 0"
    }
  ],
  "statusCode": 400
}
```

#### Not Found (404)
```json
{
  "error": "Resource not found",
  "message": "Product with ID 123e4567-e89b-12d3-a456-426614174000 was not found",
  "statusCode": 404
}
```

#### Domain Error (409 Conflict)
```json
{
  "error": "Business rule violation",
  "message": "Cannot cancel order that has already been shipped",
  "statusCode": 409
}
```

## Products API

### Get Products

Retrieve products with pagination and filtering options.

**Endpoint**: `GET /api/v1.0/products`

**Query Parameters**:
- `searchTerm` (string, optional): Search in product name or description
- `categoryId` (guid, optional): Filter by category ID
- `minPrice` (decimal, optional): Minimum price filter
- `maxPrice` (decimal, optional): Maximum price filter
- `inStockOnly` (boolean, optional): Show only products in stock
- `featuredOnly` (boolean, optional): Show only featured products
- `tags` (array of strings, optional): Filter by product tags
- `minRating` (decimal, optional): Minimum rating filter (0-5)
- `sortBy` (string, optional): Sort criteria
  - `relevance` (default)
  - `price_asc`
  - `price_desc`
  - `rating`
  - `newest`
- `page` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 20, max: 100)

**Example Request**:
```bash
GET /api/v1.0/products?searchTerm=wireless&minPrice=50&maxPrice=500&page=1&pageSize=10&sortBy=price_asc
```

**Example Response**:
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "Wireless Headphones",
      "description": "High-quality wireless headphones with noise cancellation",
      "price": 199.99,
      "currency": "USD",
      "stockQuantity": 50,
      "categoryId": "456e7890-e89b-12d3-a456-426614174001",
      "categoryName": "Electronics",
      "averageRating": 4.5,
      "reviewCount": 128,
      "tags": ["wireless", "audio", "bluetooth"],
      "isFeatured": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:45:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

### Search Products

Advanced product search using Elasticsearch with faceted results.

**Endpoint**: `GET /api/v1.0/products/search`

**Query Parameters**:
- `query` (string, required): Search query
- All parameters from Get Products endpoint

**Example Request**:
```bash
GET /api/v1.0/products/search?query=wireless headphones&categoryId=456e7890-e89b-12d3-a456-426614174001
```

**Example Response**:
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "Wireless Headphones",
      "description": "High-quality wireless headphones",
      "price": 199.99,
      "currency": "USD",
      "stockQuantity": 50,
      "relevanceScore": 0.95
    }
  ],
  "facets": {
    "categories": [
      {
        "id": "456e7890-e89b-12d3-a456-426614174001",
        "name": "Electronics",
        "count": 15
      }
    ],
    "priceRanges": [
      {
        "range": "0-100",
        "count": 5
      },
      {
        "range": "100-500",
        "count": 10
      }
    ],
    "brands": [
      {
        "name": "Sony",
        "count": 8
      }
    ]
  },
  "totalCount": 15,
  "page": 1,
  "pageSize": 20
}
```

### Create Product

Create a new product.

**Endpoint**: `POST /api/v1.0/products`

**Request Body**:
```json
{
  "name": "Wireless Headphones",
  "description": "High-quality wireless headphones with noise cancellation",
  "price": 199.99,
  "currency": "USD",
  "stockQuantity": 50,
  "categoryId": "456e7890-e89b-12d3-a456-426614174001",
  "tags": ["wireless", "audio", "bluetooth"],
  "isFeatured": false
}
```

**Response**: `201 Created`
```json
"123e4567-e89b-12d3-a456-426614174000"
```

### Update Product

Update an existing product.

**Endpoint**: `PUT /api/v1.0/products/{id}`

**Request Body**:
```json
{
  "productId": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Updated Wireless Headphones",
  "description": "Updated description",
  "price": 179.99,
  "currency": "USD",
  "stockQuantity": 75,
  "categoryId": "456e7890-e89b-12d3-a456-426614174001",
  "tags": ["wireless", "audio", "bluetooth", "updated"],
  "isFeatured": true
}
```

**Response**: `204 No Content`

### Delete Product

Delete a product.

**Endpoint**: `DELETE /api/v1.0/products/{id}`

**Response**: `204 No Content`

## Orders API

### Get Orders

Retrieve orders with pagination and filtering.

**Endpoint**: `GET /api/v1.0/orders`

**Query Parameters**:
- `searchTerm` (string, optional): Search in order details
- `customerId` (guid, optional): Filter by customer ID
- `status` (string, optional): Filter by order status
  - `pending`
  - `confirmed`
  - `processing`
  - `shipped`
  - `delivered`
  - `cancelled`
- `minAmount` (decimal, optional): Minimum order amount
- `maxAmount` (decimal, optional): Maximum order amount
- `startDate` (datetime, optional): Start date filter
- `endDate` (datetime, optional): End date filter
- `paymentMethod` (string, optional): Filter by payment method
- `paymentStatus` (string, optional): Filter by payment status
- `sortBy` (string, optional): Sort criteria
  - `created_desc` (default)
  - `created_asc`
  - `amount_desc`
  - `amount_asc`
- `page` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 20, max: 100)

**Example Response**:
```json
{
  "items": [
    {
      "id": "789e0123-e89b-12d3-a456-426614174002",
      "orderNumber": "ORD-2024-001",
      "customerId": "abc1234-e89b-12d3-a456-426614174003",
      "customerName": "John Doe",
      "status": "confirmed",
      "totalAmount": 399.98,
      "currency": "USD",
      "itemCount": 2,
      "paymentMethod": "credit_card",
      "paymentStatus": "paid",
      "shippingAddress": {
        "street": "123 Main St",
        "city": "New York",
        "state": "NY",
        "postalCode": "10001",
        "country": "USA"
      },
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T11:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

### Get Order

Retrieve a specific order by ID.

**Endpoint**: `GET /api/v1.0/orders/{id}`

**Example Response**:
```json
{
  "id": "789e0123-e89b-12d3-a456-426614174002",
  "orderNumber": "ORD-2024-001",
  "customerId": "abc1234-e89b-12d3-a456-426614174003",
  "customerName": "John Doe",
  "customerEmail": "john.doe@example.com",
  "status": "confirmed",
  "totalAmount": 399.98,
  "currency": "USD",
  "items": [
    {
      "id": "item1-789e0123-e89b-12d3-a456-426614174002",
      "productId": "123e4567-e89b-12d3-a456-426614174000",
      "productName": "Wireless Headphones",
      "quantity": 2,
      "unitPrice": 199.99,
      "totalPrice": 399.98
    }
  ],
  "payment": {
    "id": "pay1-789e0123-e89b-12d3-a456-426614174002",
    "method": "credit_card",
    "status": "paid",
    "amount": 399.98,
    "currency": "USD",
    "transactionId": "txn_1234567890",
    "processedAt": "2024-01-15T10:35:00Z"
  },
  "shippingAddress": {
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  },
  "billingAddress": {
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  },
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T11:00:00Z"
}
```

### Place Order

Create a new order.

**Endpoint**: `POST /api/v1.0/orders`

**Request Body**:
```json
{
  "customerId": "abc1234-e89b-12d3-a456-426614174003",
  "items": [
    {
      "productId": "123e4567-e89b-12d3-a456-426614174000",
      "quantity": 2,
      "unitPrice": 199.99
    }
  ],
  "shippingAddress": {
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  },
  "billingAddress": {
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  },
  "paymentMethod": "credit_card",
  "notes": "Please handle with care"
}
```

**Response**: `201 Created`
```json
"789e0123-e89b-12d3-a456-426614174002"
```

### Update Order Status

Update the status of an existing order.

**Endpoint**: `PUT /api/v1.0/orders/{id}/status`

**Request Body**:
```json
{
  "orderId": "789e0123-e89b-12d3-a456-426614174002",
  "status": "shipped",
  "notes": "Order shipped via FedEx",
  "trackingNumber": "1234567890"
}
```

**Response**: `204 No Content`

### Cancel Order

Cancel an existing order.

**Endpoint**: `POST /api/v1.0/orders/{id}/cancel`

**Request Body**:
```json
{
  "orderId": "789e0123-e89b-12d3-a456-426614174002",
  "reason": "Customer requested cancellation",
  "refundAmount": 399.98
}
```

**Response**: `204 No Content`

### Get Customer Orders

Retrieve orders for a specific customer.

**Endpoint**: `GET /api/v1.0/orders/customer/{customerId}`

**Query Parameters**:
- `status` (string, optional): Filter by order status
- `startDate` (datetime, optional): Start date filter
- `endDate` (datetime, optional): End date filter
- `sortBy` (string, optional): Sort criteria (default: `created_desc`)
- `page` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 20, max: 100)

## Customers API

### Get Customers

Retrieve customers with pagination and filtering.

**Endpoint**: `GET /api/v1.0/customers`

**Query Parameters**:
- `searchTerm` (string, optional): Search in customer name or email
- `email` (string, optional): Filter by email address
- `phoneNumber` (string, optional): Filter by phone number
- `isActive` (boolean, optional): Filter by active status
- `segment` (string, optional): Filter by customer segment
- `country` (string, optional): Filter by country
- `state` (string, optional): Filter by state
- `city` (string, optional): Filter by city
- `registrationStartDate` (datetime, optional): Registration start date
- `registrationEndDate` (datetime, optional): Registration end date
- `minLifetimeValue` (decimal, optional): Minimum lifetime value
- `maxLifetimeValue` (decimal, optional): Maximum lifetime value
- `minOrders` (integer, optional): Minimum number of orders
- `maxOrders` (integer, optional): Maximum number of orders
- `preferredLanguage` (string, optional): Filter by preferred language
- `sortBy` (string, optional): Sort criteria
  - `registration_desc` (default)
  - `registration_asc`
  - `name_asc`
  - `name_desc`
  - `lifetime_value_desc`
- `page` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 20, max: 100)

**Example Response**:
```json
{
  "items": [
    {
      "id": "abc1234-e89b-12d3-a456-426614174003",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john.doe@example.com",
      "phoneNumber": "+1-555-123-4567",
      "isActive": true,
      "segment": "premium",
      "lifetimeValue": 1250.00,
      "totalOrders": 8,
      "averageOrderValue": 156.25,
      "lastOrderDate": "2024-01-10T15:30:00Z",
      "registrationDate": "2023-06-15T09:00:00Z",
      "preferredLanguage": "en-US",
      "addresses": [
        {
          "id": "addr1-abc1234-e89b-12d3-a456-426614174003",
          "type": "shipping",
          "street": "123 Main St",
          "city": "New York",
          "state": "NY",
          "postalCode": "10001",
          "country": "USA",
          "isDefault": true
        }
      ]
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

### Get Customer

Retrieve a specific customer by ID.

**Endpoint**: `GET /api/v1.0/customers/{id}`

**Example Response**:
```json
{
  "id": "abc1234-e89b-12d3-a456-426614174003",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1-555-123-4567",
  "isActive": true,
  "segment": "premium",
  "profile": {
    "dateOfBirth": "1985-03-15",
    "gender": "male",
    "preferredLanguage": "en-US",
    "marketingOptIn": true,
    "newsletterSubscription": true
  },
  "addresses": [
    {
      "id": "addr1-abc1234-e89b-12d3-a456-426614174003",
      "type": "shipping",
      "street": "123 Main St",
      "city": "New York",
      "state": "NY",
      "postalCode": "10001",
      "country": "USA",
      "isDefault": true
    },
    {
      "id": "addr2-abc1234-e89b-12d3-a456-426614174003",
      "type": "billing",
      "street": "456 Oak Ave",
      "city": "New York",
      "state": "NY",
      "postalCode": "10002",
      "country": "USA",
      "isDefault": false
    }
  ],
  "statistics": {
    "lifetimeValue": 1250.00,
    "totalOrders": 8,
    "averageOrderValue": 156.25,
    "lastOrderDate": "2024-01-10T15:30:00Z",
    "favoriteCategories": ["Electronics", "Books"],
    "totalReviews": 5,
    "averageRating": 4.2
  },
  "registrationDate": "2023-06-15T09:00:00Z",
  "lastLoginDate": "2024-01-20T08:30:00Z"
}
```

### Register Customer

Register a new customer.

**Endpoint**: `POST /api/v1.0/customers`

**Request Body**:
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "phoneNumber": "+1-555-987-6543",
  "password": "SecurePassword123!",
  "profile": {
    "dateOfBirth": "1990-07-22",
    "gender": "female",
    "preferredLanguage": "en-US",
    "marketingOptIn": true,
    "newsletterSubscription": false
  },
  "address": {
    "type": "both",
    "street": "789 Pine St",
    "city": "Los Angeles",
    "state": "CA",
    "postalCode": "90210",
    "country": "USA"
  }
}
```

**Response**: `201 Created`
```json
"def5678-e89b-12d3-a456-426614174004"
```

### Update Customer Profile

Update customer profile information.

**Endpoint**: `PUT /api/v1.0/customers/{id}/profile`

**Request Body**:
```json
{
  "customerId": "abc1234-e89b-12d3-a456-426614174003",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1-555-123-4567",
  "profile": {
    "dateOfBirth": "1985-03-15",
    "gender": "male",
    "preferredLanguage": "en-US",
    "marketingOptIn": false,
    "newsletterSubscription": true
  }
}
```

**Response**: `204 No Content`

### Add Customer Address

Add a new address to a customer.

**Endpoint**: `POST /api/v1.0/customers/{id}/addresses`

**Request Body**:
```json
{
  "customerId": "abc1234-e89b-12d3-a456-426614174003",
  "type": "shipping",
  "street": "999 Broadway",
  "city": "New York",
  "state": "NY",
  "postalCode": "10003",
  "country": "USA",
  "isDefault": false
}
```

**Response**: `201 Created`
```json
"addr3-abc1234-e89b-12d3-a456-426614174003"
```

### Search Customer by Email

Find a customer by email address.

**Endpoint**: `GET /api/v1.0/customers/search/email?email={email}`

**Example**: `GET /api/v1.0/customers/search/email?email=john.doe@example.com`

### Get Customer Statistics

Retrieve detailed statistics for a customer.

**Endpoint**: `GET /api/v1.0/customers/{id}/statistics`

**Example Response**:
```json
{
  "customerId": "abc1234-e89b-12d3-a456-426614174003",
  "lifetimeValue": 1250.00,
  "totalOrders": 8,
  "averageOrderValue": 156.25,
  "totalSpent": 1250.00,
  "lastOrderDate": "2024-01-10T15:30:00Z",
  "firstOrderDate": "2023-07-01T10:15:00Z",
  "favoriteCategories": [
    {
      "categoryId": "456e7890-e89b-12d3-a456-426614174001",
      "categoryName": "Electronics",
      "orderCount": 5,
      "totalSpent": 750.00
    },
    {
      "categoryId": "789e0123-e89b-12d3-a456-426614174005",
      "categoryName": "Books",
      "orderCount": 3,
      "totalSpent": 500.00
    }
  ],
  "monthlySpending": [
    {
      "month": "2024-01",
      "totalSpent": 200.00,
      "orderCount": 1
    },
    {
      "month": "2023-12",
      "totalSpent": 350.00,
      "orderCount": 2
    }
  ],
  "totalReviews": 5,
  "averageRating": 4.2,
  "registrationDate": "2023-06-15T09:00:00Z",
  "daysSinceRegistration": 220,
  "daysSinceLastOrder": 10
}
```

## Health Checks

### Application Health

Check the overall health of the application and its dependencies.

**Endpoint**: `GET /health`

**Example Response**:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0234567",
      "description": "PostgreSQL connection is healthy"
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0123456",
      "description": "Redis connection is healthy"
    },
    "elasticsearch": {
      "status": "Healthy",
      "duration": "00:00:00.0345678",
      "description": "Elasticsearch cluster is healthy"
    },
    "rabbitmq": {
      "status": "Healthy",
      "duration": "00:00:00.0456789",
      "description": "RabbitMQ connection is healthy"
    }
  }
}
```

### Health Checks UI

Interactive health checks dashboard.

**Endpoint**: `GET /healthchecks-ui`

## Error Handling

### HTTP Status Codes

| Code | Description | Usage |
|------|-------------|-------|
| 200 | OK | Successful GET requests |
| 201 | Created | Successful POST requests |
| 204 | No Content | Successful PUT/DELETE requests |
| 400 | Bad Request | Invalid request parameters or body |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Business rule violation |
| 422 | Unprocessable Entity | Domain validation error |
| 500 | Internal Server Error | Unexpected server error |

### Common Error Scenarios

#### Validation Errors
- Missing required fields
- Invalid data formats
- Business rule violations
- Constraint violations

#### Not Found Errors
- Resource with specified ID doesn't exist
- Customer not found
- Product not found
- Order not found

#### Conflict Errors
- Duplicate email addresses
- Invalid state transitions
- Insufficient stock
- Order already cancelled

## Rate Limiting

Currently, the API does not implement rate limiting. For production deployment, consider implementing:
- Request rate limiting per IP
- API key-based rate limiting
- User-based rate limiting
- Burst protection

## Versioning

The API uses URL versioning with the format `/api/v{version}`. Current version is `v1.0`.

Future versions will maintain backward compatibility where possible, with breaking changes requiring a new version number.

## SDKs and Client Libraries

Currently, no official SDKs are provided. The API can be consumed using:
- Standard HTTP clients
- OpenAPI/Swagger generated clients
- Custom client implementations

## Support

For API support:
- Check this documentation
- Review the OpenAPI specification at `/swagger`
- Create an issue in the repository
- Check the health endpoints for service status