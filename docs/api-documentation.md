# 📚 API Documentation

## Overview

This document provides comprehensive API documentation for the E-Commerce API, including endpoints, request/response formats, authentication, and usage examples.

## 🌐 Base URL

```
Production: https://api.ecommerce.com/api/v1
Development: http://localhost:8080/api/v1
```

## 🔐 Authentication

Currently, the API uses basic authentication. Future versions will implement JWT tokens.

```http
Authorization: Bearer <your-jwt-token>
```

## 📋 Common Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "data": { ... },
  "message": "Success",
  "timestamp": "2024-01-15T10:30:00Z",
  "requestId": "123e4567-e89b-12d3-a456-426614174000"
}
```

### Error Response
```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed",
  "instance": "/api/v1/products",
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HM2V1K2J3L4M5N6",
  "extensions": {
    "errors": [
      {
        "field": "name",
        "message": "Product name is required"
      }
    ]
  }
}
```

## 🛍️ Products API

### Get All Products

Retrieve a paginated list of products with optional filtering.

```http
GET /api/v1/products
```

**Query Parameters:**
- `page` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 20, max: 100)
- `searchTerm` (string, optional): Search in product name and description
- `categoryId` (UUID, optional): Filter by category
- `minPrice` (decimal, optional): Minimum price filter
- `maxPrice` (decimal, optional): Maximum price filter
- `inStockOnly` (boolean, optional): Show only products in stock
- `featuredOnly` (boolean, optional): Show only featured products
- `sortBy` (string, optional): Sort field (name, price, rating, created)

**Example Request:**
```http
GET /api/v1/products?page=1&pageSize=10&searchTerm=smartphone&inStockOnly=true&sortBy=price
```

**Example Response:**
```json
{
  "items": [
    {
      "id": "34d0d27b-d198-4fce-986d-c3911302c33a",
      "name": "Samsung Galaxy S24",
      "description": "Latest Samsung flagship smartphone",
      "sku": "SAM-GS24-128",
      "price": 899.99,
      "currency": "USD",
      "stockQuantity": 15,
      "category": {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "name": "Smartphones",
        "description": "Mobile phones and accessories"
      },
      "isActive": true,
      "isFeatured": true,
      "averageRating": 4.5,
      "reviewCount": 128,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 150,
  "totalPages": 15
}
```

### Get Product by ID

Retrieve detailed information about a specific product.

```http
GET /api/v1/products/{id}
```

**Path Parameters:**
- `id` (UUID, required): Product identifier

**Example Request:**
```http
GET /api/v1/products/34d0d27b-d198-4fce-986d-c3911302c33a
```

**Example Response:**
```json
{
  "id": "34d0d27b-d198-4fce-986d-c3911302c33a",
  "name": "Samsung Galaxy S24",
  "description": "Latest Samsung flagship smartphone with advanced camera system",
  "sku": "SAM-GS24-128",
  "price": 899.99,
  "currency": "USD",
  "stockQuantity": 15,
  "minimumStockLevel": 5,
  "category": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "name": "Smartphones",
    "description": "Mobile phones and accessories"
  },
  "isActive": true,
  "isFeatured": true,
  "averageRating": 4.5,
  "reviewCount": 128,
  "reviews": [
    {
      "id": "review-123",
      "customerId": "customer-456",
      "rating": 5,
      "comment": "Excellent phone with great camera quality!",
      "createdAt": "2024-01-10T15:30:00Z"
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### Create Product

Create a new product (Admin only).

```http
POST /api/v1/products
```

**Request Body:**
```json
{
  "name": "iPhone 15 Pro",
  "description": "Apple's latest flagship smartphone",
  "sku": "APL-IP15P-256",
  "price": 1199.99,
  "currency": "USD",
  "stockQuantity": 50,
  "minimumStockLevel": 10,
  "categoryId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "isActive": true,
  "isFeatured": true
}
```

**Example Response:**
```json
{
  "id": "new-product-id",
  "name": "iPhone 15 Pro",
  "description": "Apple's latest flagship smartphone",
  "sku": "APL-IP15P-256",
  "price": 1199.99,
  "currency": "USD",
  "stockQuantity": 50,
  "minimumStockLevel": 10,
  "category": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "name": "Smartphones"
  },
  "isActive": true,
  "isFeatured": true,
  "averageRating": 0,
  "reviewCount": 0,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### Update Product

Update an existing product (Admin only).

```http
PUT /api/v1/products/{id}
```

**Path Parameters:**
- `id` (UUID, required): Product identifier

**Request Body:**
```json
{
  "name": "iPhone 15 Pro Max",
  "description": "Updated description",
  "price": 1299.99,
  "stockQuantity": 75,
  "isFeatured": false
}
```

### Delete Product

Soft delete a product (Admin only).

```http
DELETE /api/v1/products/{id}
```

**Path Parameters:**
- `id` (UUID, required): Product identifier

**Response:** `204 No Content`

### Search Products

Advanced product search with faceted filtering.

```http
GET /api/v1/products/search
```

**Query Parameters:**
- `query` (string, required): Search query
- `page` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 20)
- `categoryId` (UUID, optional): Filter by category
- `minPrice` (decimal, optional): Minimum price
- `maxPrice` (decimal, optional): Maximum price
- `minRating` (decimal, optional): Minimum rating
- `sortBy` (string, optional): Sort field

**Example Request:**
```http
GET /api/v1/products/search?query=smartphone&minPrice=500&maxPrice=1000&sortBy=rating
```

**Example Response:**
```json
{
  "products": [
    {
      "id": "34d0d27b-d198-4fce-986d-c3911302c33a",
      "name": "Samsung Galaxy S24",
      "price": 899.99,
      "averageRating": 4.5,
      "highlights": {
        "name": ["<mark>Samsung</mark> Galaxy S24"],
        "description": ["Latest <mark>smartphone</mark> technology"]
      }
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 20,
  "totalPages": 2,
  "facets": {
    "categories": [
      {
        "id": "cat-1",
        "name": "Smartphones",
        "count": 15
      }
    ],
    "priceRanges": [
      {
        "min": 500,
        "max": 750,
        "count": 8
      },
      {
        "min": 750,
        "max": 1000,
        "count": 12
      }
    ],
    "ratings": [
      {
        "rating": 4,
        "count": 18
      },
      {
        "rating": 5,
        "count": 7
      }
    ]
  }
}
```

## 📦 Orders API

### Get Orders

Retrieve customer orders with pagination.

```http
GET /api/v1/orders
```

**Query Parameters:**
- `page` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Items per page (default: 20)
- `status` (string, optional): Filter by order status
- `customerId` (UUID, optional): Filter by customer (Admin only)

**Example Response:**
```json
{
  "items": [
    {
      "id": "order-123",
      "orderNumber": "ORD-20240115-000001",
      "customerId": "customer-456",
      "status": "Processing",
      "totalAmount": 1299.98,
      "currency": "USD",
      "shippingAddress": "123 Main St, City, State 12345",
      "billingAddress": "123 Main St, City, State 12345",
      "orderItems": [
        {
          "id": "item-1",
          "productId": "product-789",
          "productName": "Samsung Galaxy S24",
          "quantity": 1,
          "unitPrice": 899.99,
          "currency": "USD",
          "totalPrice": 899.99
        }
      ],
      "createdAt": "2024-01-15T10:00:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 5,
  "totalPages": 1
}
```

### Get Order by ID

Retrieve detailed information about a specific order.

```http
GET /api/v1/orders/{id}
```

**Path Parameters:**
- `id` (UUID, required): Order identifier

### Place Order

Create a new order.

```http
POST /api/v1/orders
```

**Request Body:**
```json
{
  "customerId": "cb51e1ec-9cbc-47bb-931d-aaa2fb0d5b3d",
  "shippingAddress": "123 Main St, City, State 12345",
  "billingAddress": "123 Main St, City, State 12345",
  "orderItems": [
    {
      "productId": "34d0d27b-d198-4fce-986d-c3911302c33a",
      "productName": "Samsung Galaxy S24",
      "quantity": 1,
      "unitPrice": 899.99,
      "currency": "USD"
    }
  ]
}
```

**Example Response:**
```json
{
  "id": "new-order-id",
  "orderNumber": "ORD-20240115-000002",
  "customerId": "cb51e1ec-9cbc-47bb-931d-aaa2fb0d5b3d",
  "status": "Pending",
  "totalAmount": 899.99,
  "currency": "USD",
  "shippingAddress": "123 Main St, City, State 12345",
  "billingAddress": "123 Main St, City, State 12345",
  "orderItems": [
    {
      "productId": "34d0d27b-d198-4fce-986d-c3911302c33a",
      "productName": "Samsung Galaxy S24",
      "quantity": 1,
      "unitPrice": 899.99,
      "currency": "USD",
      "totalPrice": 899.99
    }
  ],
  "createdAt": "2024-01-15T11:00:00Z",
  "updatedAt": "2024-01-15T11:00:00Z"
}
```

### Update Order Status

Update the status of an existing order (Admin only).

```http
PUT /api/v1/orders/{id}/status
```

**Path Parameters:**
- `id` (UUID, required): Order identifier

**Request Body:**
```json
{
  "status": "Shipped",
  "notes": "Order shipped via FedEx. Tracking: 1234567890"
}
```

### Cancel Order

Cancel an existing order.

```http
DELETE /api/v1/orders/{id}
```

**Path Parameters:**
- `id` (UUID, required): Order identifier

**Response:** `204 No Content`

## 👥 Customers API

### Get Customers

Retrieve customer list (Admin only).

```http
GET /api/v1/customers
```

### Get Customer by ID

Retrieve customer information.

```http
GET /api/v1/customers/{id}
```

### Register Customer

Register a new customer.

```http
POST /api/v1/customers/register
```

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+1-555-123-4567"
}
```

### Update Customer Profile

Update customer information.

```http
PUT /api/v1/customers/{id}
```

### Add Customer Address

Add a new address for a customer.

```http
POST /api/v1/customers/{id}/addresses
```

**Request Body:**
```json
{
  "addressLine1": "123 Main Street",
  "addressLine2": "Apt 4B",
  "city": "New York",
  "state": "NY",
  "postalCode": "10001",
  "country": "USA",
  "isDefault": true
}
```

## 📂 Categories API

### Get Categories

Retrieve hierarchical category structure.

```http
GET /api/v1/categories
```

**Example Response:**
```json
{
  "items": [
    {
      "id": "cat-1",
      "name": "Electronics",
      "description": "Electronic devices and accessories",
      "parentCategoryId": null,
      "level": 0,
      "isActive": true,
      "isRoot": true,
      "hasChildren": true,
      "children": [
        {
          "id": "cat-2",
          "name": "Smartphones",
          "description": "Mobile phones and accessories",
          "parentCategoryId": "cat-1",
          "level": 1,
          "isActive": true,
          "isRoot": false,
          "hasChildren": false,
          "children": []
        }
      ]
    }
  ]
}
```

### Get Category by ID

Retrieve specific category information.

```http
GET /api/v1/categories/{id}
```

### Create Category

Create a new category (Admin only).

```http
POST /api/v1/categories
```

**Request Body:**
```json
{
  "name": "Tablets",
  "description": "Tablet computers and accessories",
  "parentCategoryId": "cat-1"
}
```

## 🔍 Health Checks

### Application Health

Check overall application health.

```http
GET /health
```

**Example Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0050000",
      "data": {
        "connectionString": "Host=localhost;Database=ecommerce"
      }
    },
    "elasticsearch": {
      "status": "Healthy",
      "duration": "00:00:00.0030000",
      "data": {
        "cluster": "elasticsearch",
        "status": "green"
      }
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0020000"
    },
    "rabbitmq": {
      "status": "Healthy",
      "duration": "00:00:00.0025000"
    }
  }
}
```

### Readiness Check

Check if application is ready to serve requests.

```http
GET /health/ready
```

### Liveness Check

Check if application is alive.

```http
GET /health/live
```

## 📊 Error Codes

| Status Code | Description | Example |
|-------------|-------------|---------|
| 200 | OK | Successful GET request |
| 201 | Created | Successful POST request |
| 204 | No Content | Successful DELETE request |
| 400 | Bad Request | Invalid request data |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource already exists |
| 422 | Unprocessable Entity | Validation errors |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |
| 503 | Service Unavailable | Service temporarily unavailable |

## 🚀 Rate Limiting

The API implements rate limiting to ensure fair usage:

- **Anonymous users**: 100 requests per hour
- **Authenticated users**: 1000 requests per hour
- **Admin users**: 5000 requests per hour

Rate limit headers are included in responses:
```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1642248000
```

## 📝 Request/Response Examples

### Validation Error Response

```json
{
  "title": "Validation Failed",
  "status": 422,
  "detail": "One or more validation errors occurred",
  "instance": "/api/v1/products",
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HM2V1K2J3L4M5N6",
  "extensions": {
    "errors": [
      {
        "field": "name",
        "message": "Product name is required and must be between 1 and 255 characters"
      },
      {
        "field": "price",
        "message": "Price must be greater than 0"
      },
      {
        "field": "categoryId",
        "message": "Category ID must be a valid UUID"
      }
    ]
  }
}
```

### Not Found Error Response

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Product with ID '34d0d27b-d198-4fce-986d-c3911302c33a' was not found",
  "instance": "/api/v1/products/34d0d27b-d198-4fce-986d-c3911302c33a",
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HM2V1K2J3L4M5N6"
}
```

## 🔧 SDK and Client Libraries

### JavaScript/TypeScript
```bash
npm install @ecommerce/api-client
```

```typescript
import { ECommerceApiClient } from '@ecommerce/api-client';

const client = new ECommerceApiClient({
  baseUrl: 'https://api.ecommerce.com/api/v1',
  apiKey: 'your-api-key'
});

const products = await client.products.getAll({
  page: 1,
  pageSize: 10,
  searchTerm: 'smartphone'
});
```

### C#
```bash
dotnet add package ECommerce.ApiClient
```

```csharp
using ECommerce.ApiClient;

var client = new ECommerceApiClient("https://api.ecommerce.com/api/v1", "your-api-key");
var products = await client.Products.GetAllAsync(new GetProductsQuery 
{
    Page = 1,
    PageSize = 10,
    SearchTerm = "smartphone"
});
```

## 📚 Additional Resources

- [Swagger/OpenAPI Documentation](http://localhost:8080/swagger)
- [Postman Collection](./postman/ecommerce-api.json)
- [API Changelog](./CHANGELOG.md)
- [Authentication Guide](./auth.md)
- [Rate Limiting Guide](./rate-limiting.md)