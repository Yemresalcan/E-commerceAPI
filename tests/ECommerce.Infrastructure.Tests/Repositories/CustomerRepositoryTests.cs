using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.ValueObjects;
using ECommerce.Infrastructure.Repositories;

namespace ECommerce.Infrastructure.Tests.Repositories;

public class CustomerRepositoryTests : RepositoryTestBase
{
    private CustomerRepository _repository = null!;

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new CustomerRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddCustomerToDatabase()
    {
        // Arrange
        var email = new Email("test@example.com");
        var phoneNumber = new PhoneNumber("+1234567890");
        var customer = Customer.Create("John", "Doe", email, phoneNumber);

        // Act
        await _repository.AddAsync(customer);
        await Context.SaveChangesAsync();

        // Assert
        var savedCustomer = await _repository.GetByIdAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.FirstName.Should().Be("John");
        savedCustomer.LastName.Should().Be("Doe");
        savedCustomer.Email.Value.Should().Be("test@example.com");
        savedCustomer.PhoneNumber!.Value.Should().Be("+1234567890");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnCustomerWithMatchingEmail()
    {
        // Arrange
        var email = new Email("unique@example.com");
        var customer = Customer.Create("Jane", "Smith", email);

        await _repository.AddAsync(customer);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.Email.Value.Should().Be("unique@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync(new Email("nonexistent@example.com"));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveCustomersAsync_ShouldReturnOnlyActiveCustomers()
    {
        // Arrange
        var activeCustomer = Customer.Create("Active", "Customer", new Email("active@example.com"));
        var inactiveCustomer = Customer.Create("Inactive", "Customer", new Email("inactive@example.com"));
        
        inactiveCustomer.Deactivate();

        await _repository.AddAsync(activeCustomer);
        await _repository.AddAsync(inactiveCustomer);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveCustomersAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(c => c.Id == activeCustomer.Id);
        result.Should().NotContain(c => c.Id == inactiveCustomer.Id);
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnCustomersMatchingSearchTerm()
    {
        // Arrange
        var customer1 = Customer.Create("John", "Smith", new Email("john.smith@example.com"));
        var customer2 = Customer.Create("Jane", "Johnson", new Email("jane.johnson@example.com"));
        var customer3 = Customer.Create("Bob", "Wilson", new Email("bob.wilson@example.com"));

        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.AddAsync(customer3);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("john");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Id == customer1.Id); // John Smith
        result.Should().Contain(c => c.Id == customer2.Id); // Jane Johnson
        result.Should().NotContain(c => c.Id == customer3.Id);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = new Email("existing@example.com");
        var customer = Customer.Create("Test", "Customer", email);
        
        await _repository.AddAsync(customer);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsByEmailAsync(new Email("nonexistent@example.com"));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithExcludeId_ShouldExcludeSpecifiedCustomer()
    {
        // Arrange
        var email = new Email("test@example.com");
        var customer = Customer.Create("Test", "Customer", email);
        
        await _repository.AddAsync(customer);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync(email, customer.Id);

        // Assert
        result.Should().BeFalse(); // Should return false because we're excluding the customer with this email
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPageAndTotalCount()
    {
        // Arrange
        var customers = new List<Customer>();
        
        for (int i = 1; i <= 12; i++)
        {
            var customer = Customer.Create($"Customer{i:D2}", "LastName", new Email($"customer{i:D2}@example.com"));
            customers.Add(customer);
            await _repository.AddAsync(customer);
        }
        
        await Context.SaveChangesAsync();

        // Act
        var (pagedCustomers, totalCount) = await _repository.GetPagedAsync(2, 5); // Page 2, 5 items per page

        // Assert
        totalCount.Should().Be(12);
        pagedCustomers.Should().HaveCount(5);
        
        // Customers should be ordered by first name then last name
        var customerList = pagedCustomers.OrderBy(c => c.FirstName).ToList();
        customerList[0].FirstName.Should().Be("Customer06");
        customerList[4].FirstName.Should().Be("Customer10");
    }

    [Fact]
    public async Task Update_ShouldUpdateCustomerInDatabase()
    {
        // Arrange
        var customer = Customer.Create("Original", "Name", new Email("original@example.com"));
        
        await _repository.AddAsync(customer);
        await Context.SaveChangesAsync();

        // Act
        customer.UpdateBasicInfo("Updated", "Name", new PhoneNumber("+9876543210"));
        _repository.Update(customer);
        await Context.SaveChangesAsync();

        // Assert
        var updatedCustomer = await _repository.GetByIdAsync(customer.Id);
        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.FirstName.Should().Be("Updated");
        updatedCustomer.PhoneNumber!.Value.Should().Be("+9876543210");
    }
}