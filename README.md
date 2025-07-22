# ECommerce Solution - .NET 9 DDD CQRS

A complete .NET 9 e-commerce solution built using Domain Driven Design (DDD), Command Query Responsibility Segregation (CQRS), and Clean Architecture patterns.

## Architecture

This solution follows Clean Architecture principles with clear separation of concerns:

### Core Layer
- **ECommerce.Domain**: Contains business entities, value objects, domain events, and business rules
- **ECommerce.Application**: Contains application services, command/query handlers, DTOs, and validation

### Infrastructure Layer
- **ECommerce.Infrastructure**: Contains data persistence, repositories, and external service integrations
- **ECommerce.ReadModel**: Contains read model implementations and Elasticsearch configurations

### Presentation Layer
- **ECommerce.WebAPI**: Contains API controllers, middleware, and web-specific configurations

### Test Projects
- **ECommerce.Domain.Tests**: Unit tests for domain logic
- **ECommerce.Application.Tests**: Unit tests for application services
- **ECommerce.Infrastructure.Tests**: Integration tests for infrastructure components

## Technology Stack

- **.NET 9**: Latest .NET framework with modern C# features
- **PostgreSQL**: Primary write database
- **Elasticsearch**: Read database for fast querying
- **Redis**: Caching layer
- **RabbitMQ**: Message broker for event handling
- **MediatR**: CQRS pipeline implementation
- **Entity Framework Core 9**: ORM for data access
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **xUnit**: Testing framework

## Project Structure

```
ECommerce.Solution/
├── src/
│   ├── Core/
│   │   ├── ECommerce.Domain/           # Domain entities, value objects, events
│   │   └── ECommerce.Application/      # Application services, CQRS handlers
│   ├── Infrastructure/
│   │   ├── ECommerce.Infrastructure/   # Data persistence, repositories
│   │   └── ECommerce.ReadModel/        # Read models, Elasticsearch
│   └── Presentation/
│       └── ECommerce.WebAPI/           # REST API controllers
├── tests/
│   ├── ECommerce.Domain.Tests/
│   ├── ECommerce.Application.Tests/
│   └── ECommerce.Infrastructure.Tests/
└── docker-compose.yml                  # Container orchestration
```

## Getting Started

### Prerequisites

- .NET 9 SDK
- Docker and Docker Compose
- PostgreSQL
- Redis
- Elasticsearch
- RabbitMQ

### Running the Application

1. Clone the repository
2. Navigate to the solution directory
3. Start the infrastructure services:
   ```bash
   docker-compose up -d
   ```
4. Run the application:
   ```bash
   dotnet run --project src/Presentation/ECommerce.WebAPI
   ```

### Running Tests

```bash
dotnet test
```

## Features

- Domain Driven Design with proper aggregate boundaries
- CQRS pattern for read/write separation
- Event sourcing with domain events
- Clean Architecture with dependency inversion
- Modern .NET 9 features (primary constructors, required properties, global usings)
- Comprehensive validation and error handling
- Health checks and monitoring
- API documentation with Swagger
- Containerized deployment

## Development

This solution is designed to be maintainable, testable, and scalable. Each layer has clear responsibilities and dependencies flow inward following the dependency inversion principle.

The CQRS pattern separates read and write operations, allowing for independent scaling and optimization of each concern.

## Contributing

Please follow the established patterns and ensure all tests pass before submitting changes.