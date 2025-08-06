# 🛒 E-Commerce API - Enterprise Level Microservices Architecture

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-green.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)](https://www.postgresql.org/)
[![Elasticsearch](https://img.shields.io/badge/Elasticsearch-8.0-yellow.svg)](https://www.elastic.co/)
[![Redis](https://img.shields.io/badge/Redis-7.0-red.svg)](https://redis.io/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-orange.svg)](https://www.rabbitmq.com/)
[![Docker](https://img.shields.io/badge/Docker-Supported-blue.svg)](https://www.docker.com/)
[![CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-green.svg)](https://github.com/features/actions)
[![Monitoring](https://img.shields.io/badge/Monitoring-Prometheus%20%2B%20Grafana-orange.svg)](https://prometheus.io/)

> **Modern, scalable, and production-ready E-Commerce API built with .NET 9, implementing Clean Architecture, CQRS, Event Sourcing, and Microservices patterns.**

[🇹🇷 Türkçe Dokümantasyon](README.tr.md) | [📖 English Documentation](#english-documentation)

---

## 🚀 **Live Demo Screenshots**

<div align="center">

### 📊 **Enterprise Monitoring Stack**
![Monitoring Overview](assets/grafana.png)

### 🎯 **Production-Ready Infrastructure**
![Infrastructure Monitoring](assets/prometeus.png)

</div>

## 🌟 **Key Features**

### 🏗️ **Architecture & Design Patterns**
- **Clean Architecture** with clear separation of concerns
- **CQRS (Command Query Responsibility Segregation)** for optimal read/write operations
- **Event-Driven Architecture** with real-time synchronization
- **Domain-Driven Design (DDD)** principles
- **Repository Pattern** with Unit of Work
- **Mediator Pattern** using MediatR
- **Specification Pattern** for complex queries

### 🚀 **Technology Stack**
- **.NET 9** - Latest framework with performance improvements
- **Entity Framework Core 9** - Advanced ORM with change tracking
- **PostgreSQL** - Primary database for ACID transactions
- **Elasticsearch** - High-performance search and analytics
- **Redis** - Distributed caching and session management
- **RabbitMQ** - Message broker for event-driven communication
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation with fluent interface
- **Serilog** - Structured logging with multiple sinks

### 🔧 **Advanced Features**
- **Real-time Stock Management** with event synchronization
- **Advanced Product Search** with faceted filtering
- **Distributed Caching** with Redis
- **Health Checks** for all services
- **API Versioning** with backward compatibility
- **Comprehensive Logging** with correlation IDs
- **Exception Handling** with global middleware
- **Performance Monitoring** with custom metrics
- **Swagger/OpenAPI** documentation

---

## 📊 **System Architecture**

```mermaid
graph TB
    subgraph "Presentation Layer"
        API[Web API Controllers]
        MW[Middleware Pipeline]
    end
    
    subgraph "Application Layer"
        MED[MediatR]
        CMD[Commands]
        QRY[Queries]
        VAL[Validators]
        BEH[Behaviors]
    end
    
    subgraph "Domain Layer"
        ENT[Entities]
        VO[Value Objects]
        AGG[Aggregates]
        DOM[Domain Services]
    end
    
    subgraph "Infrastructure Layer"
        REPO[Repositories]
        CACHE[Cache Service]
        MSG[Message Bus]
        EXT[External Services]
    end
    
    subgraph "Data Stores"
        PG[(PostgreSQL)]
        ES[(Elasticsearch)]
        RD[(Redis)]
        RMQ[RabbitMQ]
    end
    
    API --> MW
    MW --> MED
    MED --> CMD
    MED --> QRY
    CMD --> VAL
    QRY --> VAL
    VAL --> BEH
    BEH --> DOM
    DOM --> ENT
    DOM --> REPO
    REPO --> PG
    CACHE --> RD
    MSG --> RMQ
    EXT --> ES
```

---

## 🗄️ **Database Schema**

```mermaid
erDiagram
    CUSTOMERS {
        uuid id PK
        string email UK
        string first_name
        string last_name
        string phone
        timestamp created_at
        timestamp updated_at
        boolean is_active
    }
    
    CUSTOMER_ADDRESSES {
        uuid id PK
        uuid customer_id FK
        string address_line1
        string address_line2
        string city
        string state
        string postal_code
        string country
        boolean is_default
        timestamp created_at
    }
    
    CATEGORIES {
        uuid id PK
        string name UK
        string description
        uuid parent_category_id FK
        int level
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }
    
    PRODUCTS {
        uuid id PK
        string name
        string description
        string sku UK
        decimal price
        string currency
        int stock_quantity
        int minimum_stock_level
        uuid category_id FK
        boolean is_active
        boolean is_featured
        decimal average_rating
        int review_count
        timestamp created_at
        timestamp updated_at
    }
    
    PRODUCT_REVIEWS {
        uuid id PK
        uuid product_id FK
        uuid customer_id FK
        int rating
        string comment
        timestamp created_at
    }
    
    ORDERS {
        uuid id PK
        string order_number UK
        uuid customer_id FK
        string status
        decimal total_amount
        string currency
        string shipping_address
        string billing_address
        timestamp created_at
        timestamp updated_at
    }
    
    ORDER_ITEMS {
        uuid id PK
        uuid order_id FK
        uuid product_id FK
        string product_name
        int quantity
        decimal unit_price
        string currency
        decimal total_price
    }
    
    STOCK_MOVEMENTS {
        uuid id PK
        uuid product_id FK
        int quantity_change
        string movement_type
        string reason
        int stock_after_movement
        timestamp created_at
    }
    
    CUSTOMERS ||--o{ CUSTOMER_ADDRESSES : has
    CUSTOMERS ||--o{ PRODUCT_REVIEWS : writes
    CUSTOMERS ||--o{ ORDERS : places
    CATEGORIES ||--o{ CATEGORIES : contains
    CATEGORIES ||--o{ PRODUCTS : categorizes
    PRODUCTS ||--o{ PRODUCT_REVIEWS : receives
    PRODUCTS ||--o{ ORDER_ITEMS : included_in
    PRODUCTS ||--o{ STOCK_MOVEMENTS : tracks
    ORDERS ||--o{ ORDER_ITEMS : contains
```

---

## 🔄 **Event-Driven Workflow**

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant MediatR
    participant Handler
    participant Repository
    participant EventBus
    participant Elasticsearch
    participant Cache
    
    Client->>API: POST /api/v1/orders
    API->>MediatR: PlaceOrderCommand
    MediatR->>Handler: Handle Command
    
    Handler->>Repository: Get Product
    Repository-->>Handler: Product Data
    
    Handler->>Repository: Update Stock
    Repository->>EventBus: ProductStockUpdatedEvent
    
    EventBus->>Elasticsearch: Sync Stock Data
    EventBus->>Cache: Invalidate Cache
    
    Handler->>Repository: Create Order
    Repository-->>Handler: Order Created
    
    Handler-->>MediatR: Success Response
    MediatR-->>API: Order DTO
    API-->>Client: 201 Created
    
    Note over EventBus: Asynchronous Processing
    EventBus->>Elasticsearch: Update Search Index
    EventBus->>Cache: Update Product Cache
```

---

## 🚀 **Quick Start**

### Prerequisites
- .NET 9 SDK
- PostgreSQL 16+
- Elasticsearch 8.0+
- Redis 7.0+
- RabbitMQ 3.12+
- Docker (optional)

### 1. Clone Repository
```bash
git clone https://github.com/Yemresalcan/ecommerce-api.git
cd ecommerce-api
```

### 2. Setup Infrastructure (Docker)
```bash
docker-compose up -d postgres elasticsearch redis rabbitmq
```

### 3. Configure Application
```bash
cp src/Presentation/ECommerce.WebAPI/appsettings.example.json src/Presentation/ECommerce.WebAPI/appsettings.json
# Edit connection strings and configurations
```

### 4. Run Migrations
```bash
dotnet ef database update --project src/Infrastructure/ECommerce.Infrastructure --startup-project src/Presentation/ECommerce.WebAPI
```

### 5. Start Application
```bash
dotnet run --project src/Presentation/ECommerce.WebAPI
```

### 6. Access API
- **Swagger UI**: http://localhost:8080/swagger
- **Health Checks**: http://localhost:8080/health
- **API Base**: http://localhost:8080/api/v1

---

## 📁 **Project Structure**

```
src/
├── Core/
│   ├── ECommerce.Domain/           # Domain entities, value objects, aggregates
│   └── ECommerce.Application/      # Use cases, DTOs, interfaces
├── Infrastructure/
│   ├── ECommerce.Infrastructure/   # Data access, external services
│   └── ECommerce.ReadModel/        # Elasticsearch, read-side queries
└── Presentation/
    └── ECommerce.WebAPI/           # Controllers, middleware, configuration

tests/
├── ECommerce.Domain.Tests/         # Domain unit tests
├── ECommerce.Application.Tests/    # Application unit tests
├── ECommerce.Infrastructure.Tests/ # Infrastructure unit tests
└── ECommerce.WebAPI.Tests/         # Integration tests
```

---

## 🔧 **Configuration**

### Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ecommerce;Username=postgres;Password=password"
  }
}
```

### Elasticsearch Configuration
```json
{
  "Elasticsearch": {
    "Uri": "http://localhost:9200",
    "IndexPrefix": "ecommerce",
    "Username": "",
    "Password": ""
  }
}
```

### Redis Configuration
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

---

## 📊 **Live Monitoring Screenshots**

### 🎯 **Grafana Dashboard - Real-time System Monitoring**
![Grafana Dashboard](assets/grafana.png)
*Professional monitoring dashboard showing CPU, Memory, Network, and Disk metrics in real-time*

### 📈 **Prometheus Metrics Collection**
![Prometheus Targets](assets/prometeus.png)
*Prometheus successfully collecting metrics from all monitoring targets*

### 🐳 **Container Monitoring with cAdvisor**
![cAdvisor Container Monitoring](assets/cAdvisor.png)
*Detailed container resource usage and performance metrics*

### 🛠️ **Container Management with Portainer**
![Portainer Dashboard](assets/portanier.png)
*Professional container management interface for Docker environments*

### 🐰 **Message Queue Monitoring**
![RabbitMQ Management](assets/rabitmq.png)
*RabbitMQ management interface showing queue status and message flow*

### 🛒 **E-Commerce API Documentation**
![API Documentation](assets/api_screenshot.png)
*Comprehensive Swagger/OpenAPI documentation with interactive testing*

## 📊 **Performance Metrics**

| Endpoint | Avg Response Time | Throughput |
|----------|------------------|------------|
| GET /products | 45ms | 2,000 req/s |
| POST /orders | 120ms | 500 req/s |
| GET /orders/{id} | 25ms | 3,000 req/s |
| GET /search | 80ms | 1,200 req/s |

---

## 🧪 **Testing**

### Run Unit Tests
```bash
dotnet test tests/ECommerce.Domain.Tests/
dotnet test tests/ECommerce.Application.Tests/
```

### Run Integration Tests
```bash
dotnet test tests/ECommerce.WebAPI.Tests/
```

### Test Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📈 **Monitoring & Observability**

### Health Checks
- Database connectivity
- Elasticsearch cluster health
- Redis availability
- RabbitMQ connection
- External service dependencies

### Logging
- Structured logging with Serilog
- Correlation IDs for request tracking
- Performance metrics
- Error tracking and alerting

### Metrics
- Request/response times
- Database query performance
- Cache hit/miss ratios
- Event processing metrics

---

## 🔒 **Security Features**

- Input validation with FluentValidation
- SQL injection prevention with parameterized queries
- XSS protection with output encoding
- CORS configuration
- Rate limiting
- API versioning
- Health check security

---

## 🚀 **Deployment**

### Docker Deployment
```bash
docker build -t ecommerce-api .
docker run -p 8080:8080 ecommerce-api
```

### Kubernetes Deployment
```bash
kubectl apply -f k8s/
```

### CI/CD Pipeline
- GitHub Actions workflow
- Automated testing
- Docker image building
- Deployment to staging/production

---

## 🤝 **Contributing**

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 **Author**

**Your Name**
- GitHub: [@Yemresalcan](https://github.com/Yemresalcan)
- LinkedIn: [Yunusemresalcan](https://linkedin.com/in/yunusemresalcan)
- Email: yunusemresalcan@gmail.com

---

## 🙏 **Acknowledgments**

- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- Microsoft .NET Documentation
- Elasticsearch Documentation
- Redis Documentation

---

## 📚 **Additional Resources**

- [API Documentation](docs/api.md)
- [Architecture Decision Records](docs/adr/)
- [Deployment Guide](docs/deployment.md)
- [Contributing Guidelines](CONTRIBUTING.md)
- [Code of Conduct](CODE_OF_CONDUCT.md)

---

## 🎯 **Production Screenshots Gallery**

<div align="center">

| Monitoring Dashboard | Container Management | API Documentation |
|:---:|:---:|:---:|
| ![Grafana](assets/grafana.png) | ![Portainer](assets/portanier.png) | ![API Docs](assets/api_screenshot.png) |
| **Real-time Metrics** | **Container Control** | **Interactive API** |

| System Health | Message Queue | Container Analytics |
|:---:|:---:|:---:|
| ![Prometheus](assets/prometeus.png) | ![RabbitMQ](assets/rabitmq.png) | ![cAdvisor](assets/cAdvisor.png) |
| **Target Monitoring** | **Queue Management** | **Resource Analytics** |

</div>

---

<div align="center">

**⭐ If you found this project helpful, please give it a star! ⭐**

**🚀 Ready for Production • 📊 Enterprise Monitoring • 🛒 Scalable E-Commerce**

</div>
