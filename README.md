# ECommerce Solution - .NET 9 DDD CQRS

A complete .NET 9 e-commerce solution built using Domain Driven Design (DDD), Command Query Responsibility Segregation (CQRS), and Clean Architecture patterns. This production-ready platform demonstrates modern software architecture principles with comprehensive testing, monitoring, and containerized deployment.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Quick Start](#quick-start)
- [API Documentation](#api-documentation)
- [Development Setup](#development-setup)
- [Testing](#testing)
- [Deployment](#deployment)
- [Architecture Details](#architecture-details)
- [Contributing](#contributing)

## Architecture Overview

This solution implements a sophisticated e-commerce platform using:

- **Domain Driven Design (DDD)**: Business logic encapsulated in domain aggregates
- **CQRS Pattern**: Separate read and write operations for optimal performance
- **Clean Architecture**: Dependency inversion with clear layer separation
- **Event Sourcing**: Domain events for system integration and audit trails
- **Microservices Ready**: Modular design supporting distributed deployment

### Core Principles

- **Aggregate Boundaries**: Product, Order, and Customer aggregates with proper invariants
- **Command/Query Separation**: Write operations to PostgreSQL, read operations from Elasticsearch
- **Event-Driven Architecture**: Asynchronous processing with RabbitMQ
- **Eventual Consistency**: Read models updated via domain events
- **Modern .NET 9**: Primary constructors, required properties, global usings

## Technology Stack

### Core Framework
- **.NET 9**: Latest framework with performance improvements
- **C# 13**: Modern language features and syntax

### Data Storage
- **PostgreSQL 16**: Primary write database with ACID compliance
- **Elasticsearch 8**: Fast search and read operations
- **Redis 7**: Distributed caching and session storage

### Messaging & Integration
- **RabbitMQ**: Reliable message broker for event handling
- **MediatR**: In-process messaging for CQRS pipeline

### Development Tools
- **Entity Framework Core 9**: Modern ORM with migrations
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Declarative validation rules
- **Serilog**: Structured logging with multiple sinks

### Testing & Quality
- **xUnit**: Unit and integration testing framework
- **Testcontainers**: Integration testing with real databases
- **FluentAssertions**: Readable test assertions
- **Moq**: Mocking framework for unit tests

### DevOps & Monitoring
- **Docker**: Containerization for all services
- **Health Checks**: Built-in monitoring endpoints
- **Swagger/OpenAPI**: Interactive API documentation
- **Prometheus**: Metrics collection (configurable)

## Project Structure

```
ECommerce.Solution/
├── src/
│   ├── Core/                           # Business Logic Layer
│   │   ├── ECommerce.Domain/           # Domain entities, value objects, events
│   │   │   ├── Aggregates/             # Product, Order, Customer aggregates
│   │   │   ├── ValueObjects/           # Money, Email, PhoneNumber
│   │   │   ├── Events/                 # Domain events
│   │   │   └── Exceptions/             # Domain-specific exceptions
│   │   └── ECommerce.Application/      # Application Services Layer
│   │       ├── Commands/               # Write operations (CQRS)
│   │       ├── Queries/                # Read operations (CQRS)
│   │       ├── Handlers/               # Command/Query handlers
│   │       ├── DTOs/                   # Data transfer objects
│   │       ├── Validators/             # FluentValidation rules
│   │       └── Mappings/               # AutoMapper profiles
│   ├── Infrastructure/                 # External Concerns Layer
│   │   ├── ECommerce.Infrastructure/   # Data persistence, repositories
│   │   │   ├── Persistence/            # EF Core configurations
│   │   │   ├── Repositories/           # Repository implementations
│   │   │   ├── Services/               # External service integrations
│   │   │   ├── Messaging/              # RabbitMQ implementations
│   │   │   └── Caching/                # Redis caching services
│   │   └── ECommerce.ReadModel/        # Read Model Layer
│   │       ├── Models/                 # Elasticsearch document models
│   │       ├── Services/               # Search service implementations
│   │       └── Configurations/         # Index mappings and settings
│   └── Presentation/                   # API Layer
│       └── ECommerce.WebAPI/           # REST API controllers
│           ├── Controllers/            # API endpoints
│           ├── Middleware/             # Custom middleware
│           ├── Extensions/             # Service registration extensions
│           └── Configuration/          # Startup configurations
├── tests/                              # Test Projects
│   ├── ECommerce.Domain.Tests/         # Domain logic unit tests
│   ├── ECommerce.Application.Tests/    # Application service tests
│   ├── ECommerce.Infrastructure.Tests/ # Infrastructure integration tests
│   └── ECommerce.WebAPI.Tests/         # API endpoint tests
├── scripts/                            # Build and deployment scripts
├── rabbitmq/                           # RabbitMQ configuration files
├── docker-compose.yml                  # Development environment
├── docker-compose.prod.yml             # Production environment
└── Dockerfile                          # Application container definition
```

## Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### 1. Clone and Setup

```bash
# Clone the repository
git clone <repository-url>
cd ECommerce.Solution

# Copy environment configuration
cp .env.example .env
```

### 2. Start Infrastructure Services

```bash
# Start all services with Docker Compose
docker-compose up -d

# Or use the provided scripts
# Linux/Mac/WSL:
./scripts/docker-run-dev.sh

# Windows PowerShell:
.\scripts\docker-run-dev.ps1
```

### 3. Run the Application

```bash
# Restore dependencies
dotnet restore

# Run the API
dotnet run --project src/Presentation/ECommerce.WebAPI

# Or build and run with Docker
docker-compose up --build ecommerce-api
```

### 4. Access the Application

- **API Base URL**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Health Checks**: http://localhost:8080/health
- **Health Checks UI**: http://localhost:8080/healthchecks-ui

### 5. Verify Setup

```bash
# Check all services are running
docker-compose ps

# Test API health
curl http://localhost:8080/health

# View application logs
docker-compose logs -f ecommerce-api
```

## API Documentation

### Base URL
- **Development**: `http://localhost:8080`
- **API Version**: `v1.0`
- **Base Path**: `/api/v1.0`

### Authentication
Currently, the API does not implement authentication. In production, consider implementing:
- JWT Bearer tokens
- OAuth 2.0 / OpenID Connect
- API Key authentication

### Core Endpoints

#### Products API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1.0/products` | Get products with pagination and filtering |
| GET | `/api/v1.0/products/search` | Advanced product search |
| POST | `/api/v1.0/products` | Create a new product |
| PUT | `/api/v1.0/products/{id}` | Update an existing product |
| DELETE | `/api/v1.0/products/{id}` | Delete a product |

#### Orders API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1.0/orders` | Get orders with pagination and filtering |
| GET | `/api/v1.0/orders/{id}` | Get a specific order |
| GET | `/api/v1.0/orders/customer/{customerId}` | Get customer orders |
| POST | `/api/v1.0/orders` | Place a new order |
| PUT | `/api/v1.0/orders/{id}/status` | Update order status |
| POST | `/api/v1.0/orders/{id}/cancel` | Cancel an order |

#### Customers API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1.0/customers` | Get customers with pagination and filtering |
| GET | `/api/v1.0/customers/{id}` | Get a specific customer |
| GET | `/api/v1.0/customers/{id}/statistics` | Get customer statistics |
| GET | `/api/v1.0/customers/search/email` | Search customer by email |
| POST | `/api/v1.0/customers` | Register a new customer |
| PUT | `/api/v1.0/customers/{id}/profile` | Update customer profile |
| POST | `/api/v1.0/customers/{id}/addresses` | Add customer address |

### Example API Usage

#### Create a Product
```bash
curl -X POST "http://localhost:8080/api/v1.0/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Wireless Headphones",
    "description": "High-quality wireless headphones with noise cancellation",
    "price": 199.99,
    "currency": "USD",
    "stockQuantity": 50,
    "categoryId": "123e4567-e89b-12d3-a456-426614174000"
  }'
```

#### Search Products
```bash
curl "http://localhost:8080/api/v1.0/products/search?query=headphones&page=1&pageSize=10&sortBy=relevance"
```

#### Place an Order
```bash
curl -X POST "http://localhost:8080/api/v1.0/orders" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "123e4567-e89b-12d3-a456-426614174001",
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
    }
  }'
```

### Response Formats

#### Success Response
```json
{
  "data": { /* response data */ },
  "success": true,
  "message": "Operation completed successfully"
}
```

#### Error Response
```json
{
  "error": "Validation failed",
  "details": [
    {
      "field": "email",
      "message": "Email is required"
    }
  ],
  "statusCode": 400
}
```

#### Paginated Response
```json
{
  "items": [ /* array of items */ ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## Development Setup

### Local Development Environment

1. **Install Prerequisites**
   ```bash
   # Install .NET 9 SDK
   winget install Microsoft.DotNet.SDK.9
   
   # Install Docker Desktop
   winget install Docker.DockerDesktop
   
   # Install Git
   winget install Git.Git
   ```

2. **IDE Setup**
   - **Visual Studio 2022**: Latest version with .NET 9 support
   - **VS Code**: With C# extension
   - **JetBrains Rider**: 2024.3 or later

3. **Database Tools** (Optional)
   - **pgAdmin**: PostgreSQL administration
   - **Redis Insight**: Redis management
   - **Elasticsearch Head**: Elasticsearch management

### Environment Configuration

Create a `.env` file in the project root:

```env
# Database Configuration
POSTGRES_DB=ecommerce
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_HOST=localhost
POSTGRES_PORT=5432

# Redis Configuration
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=

# Elasticsearch Configuration
ELASTICSEARCH_HOST=localhost
ELASTICSEARCH_PORT=9200

# RabbitMQ Configuration
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
API_PORT=8080
```

### Development Workflow

1. **Start Infrastructure**
   ```bash
   docker-compose up -d postgres redis elasticsearch rabbitmq
   ```

2. **Run Migrations**
   ```bash
   dotnet ef database update --project src/Infrastructure/ECommerce.Infrastructure
   ```

3. **Start the API**
   ```bash
   dotnet run --project src/Presentation/ECommerce.WebAPI
   ```

4. **Run Tests**
   ```bash
   # Unit tests
   dotnet test tests/ECommerce.Domain.Tests/
   
   # Integration tests
   dotnet test tests/ECommerce.Infrastructure.Tests/
   
   # All tests
   dotnet test
   ```

## Testing

### Test Strategy

The solution implements a comprehensive testing strategy:

- **Unit Tests**: Fast, isolated tests for business logic
- **Integration Tests**: Tests with real databases using Testcontainers
- **API Tests**: End-to-end testing of HTTP endpoints
- **Performance Tests**: Load testing for critical paths

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/ECommerce.Domain.Tests/

# Run tests with filter
dotnet test --filter "Category=Unit"

# Run tests in parallel
dotnet test --parallel
```

### Test Categories

- **Unit**: Fast, isolated unit tests
- **Integration**: Tests requiring database/external services
- **E2E**: End-to-end API tests
- **Performance**: Load and stress tests

### Test Data

Tests use:
- **In-memory databases** for unit tests
- **Testcontainers** for integration tests
- **Test data builders** for consistent test data
- **Fixtures** for shared test setup

## Deployment

### Docker Deployment

#### Development
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

#### Production
```bash
# Build production images
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build

# Start production environment
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Monitor services
docker-compose ps
docker stats
```

### Environment-Specific Configurations

#### Development
- Debug logging enabled
- Swagger UI available
- Hot reload for code changes
- In-memory caching for faster development

#### Production
- Optimized logging levels
- Health checks enabled
- Resource limits configured
- Security headers enabled
- Connection pooling optimized

### Scaling Considerations

#### Horizontal Scaling
- **API**: Stateless design allows multiple instances
- **Database**: Read replicas for query scaling
- **Cache**: Redis clustering for high availability
- **Search**: Elasticsearch cluster for performance
- **Messaging**: RabbitMQ clustering for reliability

#### Monitoring
- Health check endpoints for all services
- Structured logging with correlation IDs
- Metrics collection (Prometheus compatible)
- Distributed tracing support

## Architecture Details

### Domain Driven Design

#### Aggregates
- **Product Aggregate**: Product, Category, ProductReview
- **Order Aggregate**: Order, OrderItem, Payment
- **Customer Aggregate**: Customer, Address, Profile

#### Value Objects
- **Money**: Amount and currency with arithmetic operations
- **Email**: Email validation and formatting
- **PhoneNumber**: Phone number validation and formatting

#### Domain Events
- **ProductCreatedEvent**: Published when products are created
- **OrderPlacedEvent**: Published when orders are placed
- **CustomerRegisteredEvent**: Published when customers register

### CQRS Implementation

#### Command Side (Write)
- Commands processed by aggregate roots
- Data persisted to PostgreSQL
- Domain events published to RabbitMQ
- Strong consistency within aggregates

#### Query Side (Read)
- Read models in Elasticsearch
- Optimized for query performance
- Eventually consistent with write side
- Supports complex search scenarios

### Event Sourcing

#### Event Flow
1. Command executed on aggregate
2. Domain events generated
3. Events persisted with aggregate state
4. Events published to message bus
5. Event handlers update read models

#### Benefits
- Complete audit trail
- Temporal queries possible
- Easy to add new projections
- Natural fit with CQRS

## Contributing

### Development Guidelines

1. **Follow Clean Architecture principles**
2. **Write tests for all business logic**
3. **Use meaningful commit messages**
4. **Follow C# coding conventions**
5. **Update documentation for API changes**

### Code Style

- Use modern C# features (primary constructors, required properties)
- Follow SOLID principles
- Implement proper error handling
- Use async/await consistently
- Write self-documenting code

### Pull Request Process

1. Create feature branch from main
2. Implement changes with tests
3. Ensure all tests pass
4. Update documentation
5. Submit pull request with description
6. Address review feedback
7. Merge after approval

### Getting Help

- Check existing documentation
- Review test examples
- Ask questions in issues
- Follow established patterns in codebase

---

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:
- Create an issue in the repository
- Check the documentation
- Review the test examples
- Follow the established patterns
