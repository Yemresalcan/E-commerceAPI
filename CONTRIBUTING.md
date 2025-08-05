# 🤝 Contributing to E-Commerce API

Thank you for your interest in contributing to the E-Commerce API project! This document provides guidelines and information for contributors.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Process](#contributing-process)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting](#issue-reporting)

## 📜 Code of Conduct

This project adheres to a [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## 🚀 Getting Started

### Prerequisites

Before contributing, ensure you have:

- .NET 9 SDK installed
- PostgreSQL 16+ running
- Elasticsearch 8.0+ running
- Redis 7.0+ running
- RabbitMQ 3.12+ running
- Git for version control
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/Yemresalcan/ecommerce-api.git
   cd ecommerce-api
   ```
3. Add the upstream repository:
   ```bash
   git remote add upstream https://github.com/originalowner/ecommerce-api.git
   ```

## 🛠️ Development Setup

### 1. Infrastructure Setup

Start the required services using Docker Compose:

```bash
docker-compose up -d postgres elasticsearch redis rabbitmq
```

### 2. Database Setup

Run the database migrations:

```bash
dotnet ef database update --project src/Infrastructure/ECommerce.Infrastructure --startup-project src/Presentation/ECommerce.WebAPI
```

### 3. Configuration

Copy the example configuration:

```bash
cp src/Presentation/ECommerce.WebAPI/appsettings.example.json src/Presentation/ECommerce.WebAPI/appsettings.Development.json
```

Update the connection strings and settings as needed.

### 4. Build and Run

```bash
dotnet build
dotnet run --project src/Presentation/ECommerce.WebAPI
```

## 🔄 Contributing Process

### 1. Create a Branch

Create a feature branch from `develop`:

```bash
git checkout develop
git pull upstream develop
git checkout -b feature/your-feature-name
```

### Branch Naming Convention

- `feature/` - New features
- `bugfix/` - Bug fixes
- `hotfix/` - Critical fixes for production
- `docs/` - Documentation updates
- `refactor/` - Code refactoring
- `test/` - Test improvements

### 2. Make Changes

- Write clean, maintainable code
- Follow the coding standards
- Add tests for new functionality
- Update documentation as needed

### 3. Commit Changes

Use conventional commit messages:

```bash
git commit -m "feat: add product search functionality"
git commit -m "fix: resolve stock calculation issue"
git commit -m "docs: update API documentation"
```

### Commit Message Format

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Test changes
- `chore`: Build/tooling changes

## 📝 Coding Standards

### C# Coding Standards

#### Naming Conventions

```csharp
// Classes: PascalCase
public class ProductService { }

// Methods: PascalCase
public async Task<Product> GetProductAsync(Guid id) { }

// Properties: PascalCase
public string ProductName { get; set; }

// Fields: camelCase with underscore prefix
private readonly ILogger _logger;

// Constants: PascalCase
public const int MaxRetryAttempts = 3;

// Local variables: camelCase
var productId = Guid.NewGuid();
```

#### Code Structure

```csharp
// File header (optional)
// Copyright (c) 2024 E-Commerce API. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// ... other using statements

namespace ECommerce.Application.Services;

/// <summary>
/// Service for managing product operations
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting product with ID: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException("Product", id);
        }

        return product;
    }
}
```

#### Best Practices

1. **Use async/await consistently**
   ```csharp
   public async Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
   {
       return await _productRepository.GetByIdAsync(id, cancellationToken);
   }
   ```

2. **Implement proper error handling**
   ```csharp
   try
   {
       var result = await SomeOperationAsync();
       return result;
   }
   catch (SpecificException ex)
   {
       _logger.LogError(ex, "Specific error occurred");
       throw new DomainException("User-friendly message", ex);
   }
   ```

3. **Use dependency injection**
   ```csharp
   public class ProductService
   {
       private readonly IProductRepository _repository;
       
       public ProductService(IProductRepository repository)
       {
           _repository = repository ?? throw new ArgumentNullException(nameof(repository));
       }
   }
   ```

4. **Add XML documentation**
   ```csharp
   /// <summary>
   /// Retrieves a product by its unique identifier
   /// </summary>
   /// <param name="id">The product identifier</param>
   /// <param name="cancellationToken">Cancellation token</param>
   /// <returns>The product if found</returns>
   /// <exception cref="NotFoundException">Thrown when product is not found</exception>
   public async Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
   ```

### Code Quality Tools

We use the following tools to maintain code quality:

- **EditorConfig**: Consistent coding style
- **StyleCop**: C# style rules
- **SonarAnalyzer**: Code quality analysis
- **Roslynator**: Additional analyzers

## 🧪 Testing Guidelines

### Test Structure

Follow the AAA pattern (Arrange, Act, Assert):

```csharp
[Test]
public async Task GetProductAsync_WithValidId_ReturnsProduct()
{
    // Arrange
    var productId = Guid.NewGuid();
    var expectedProduct = new Product { Id = productId, Name = "Test Product" };
    _mockRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedProduct);

    // Act
    var result = await _productService.GetProductAsync(productId);

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Id, Is.EqualTo(productId));
    Assert.That(result.Name, Is.EqualTo("Test Product"));
}
```

### Test Categories

1. **Unit Tests**: Test individual components in isolation
2. **Integration Tests**: Test component interactions
3. **End-to-End Tests**: Test complete user scenarios

### Test Naming Convention

```
MethodName_Scenario_ExpectedResult
```

Examples:
- `GetProductAsync_WithValidId_ReturnsProduct`
- `CreateOrderAsync_WithInsufficientStock_ThrowsException`
- `UpdateProductAsync_WithInvalidData_ReturnsValidationError`

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/ECommerce.Application.Tests/

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests in watch mode
dotnet watch test
```

## 📚 Documentation

### Code Documentation

- Add XML documentation for public APIs
- Include examples in documentation
- Document complex business logic
- Keep documentation up to date

### API Documentation

- Update Swagger/OpenAPI documentation
- Include request/response examples
- Document error scenarios
- Update postman collections

### Architecture Documentation

- Update architecture diagrams
- Document design decisions
- Maintain ADR (Architecture Decision Records)
- Update deployment guides

## 🔍 Pull Request Process

### Before Submitting

1. **Sync with upstream**:
   ```bash
   git checkout develop
   git pull upstream develop
   git checkout your-feature-branch
   git rebase develop
   ```

2. **Run tests**:
   ```bash
   dotnet test
   ```

3. **Check code quality**:
   ```bash
   dotnet build --configuration Release
   ```

4. **Update documentation** if needed

### Pull Request Template

When creating a pull request, use this template:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No breaking changes (or documented)
```

### Review Process

1. **Automated checks** must pass
2. **Code review** by maintainers
3. **Testing** in staging environment
4. **Approval** from code owners
5. **Merge** to develop branch

## 🐛 Issue Reporting

### Bug Reports

Use the bug report template:

```markdown
**Describe the bug**
A clear description of the bug

**To Reproduce**
Steps to reproduce the behavior

**Expected behavior**
What you expected to happen

**Screenshots**
If applicable, add screenshots

**Environment**
- OS: [e.g. Windows 11]
- .NET Version: [e.g. 9.0]
- Browser: [e.g. Chrome 120]

**Additional context**
Any other context about the problem
```

### Feature Requests

Use the feature request template:

```markdown
**Is your feature request related to a problem?**
A clear description of the problem

**Describe the solution you'd like**
A clear description of what you want to happen

**Describe alternatives you've considered**
Alternative solutions or features

**Additional context**
Any other context or screenshots
```

## 🏷️ Release Process

### Versioning

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Release Branches

- `main`: Production releases
- `develop`: Development branch
- `release/x.y.z`: Release preparation

### Release Checklist

- [ ] Update version numbers
- [ ] Update CHANGELOG.md
- [ ] Run full test suite
- [ ] Update documentation
- [ ] Create release notes
- [ ] Tag release
- [ ] Deploy to production

## 🆘 Getting Help

### Communication Channels

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: General questions and discussions
- **Slack**: Real-time communication (invite required)
- **Email**: maintainers@ecommerce-api.com

### Resources

- [Architecture Documentation](docs/architecture.md)
- [API Documentation](docs/api-documentation.md)
- [Database Schema](docs/database-schema.md)
- [Deployment Guide](docs/deployment.md)

## 🙏 Recognition

Contributors will be recognized in:

- README.md contributors section
- Release notes
- Annual contributor report
- Conference presentations (with permission)

Thank you for contributing to the E-Commerce API project! 🚀