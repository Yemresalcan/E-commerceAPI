using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.ValueObjects;
using ECommerce.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>
/// Entity Framework DbContext for the e-commerce application
/// </summary>
public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    // Product Aggregate
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    // Order Aggregate
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Customer Aggregate
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Profile> Profiles => Set<Profile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore DomainEvent base class - it's not a database entity
        modelBuilder.Ignore<ECommerce.Domain.Events.DomainEvent>();

        // Configure Value Objects as Owned Types
        ConfigureValueObjects(modelBuilder);

        // Apply all entity configurations with performance optimizations
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductReviewConfiguration());
        
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        // modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        // modelBuilder.ApplyConfiguration(new AddressConfiguration());
        // modelBuilder.ApplyConfiguration(new ProfileConfiguration());

        // Configure schema
        modelBuilder.HasDefaultSchema("ecommerce");
    }

    private void ConfigureValueObjects(ModelBuilder modelBuilder)
    {
        // Configure Money value object
        modelBuilder.Entity<Product>()
            .OwnsOne(p => p.Price, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Price_Amount");
                money.Property(m => m.Currency).HasColumnName("Price_Currency").HasMaxLength(3);
            });

        // Configure Email value object for Customer
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.Email, email =>
            {
                email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(255);
            });

        // Email index will be added manually due to owned type complexity

        // Configure PhoneNumber value object for Customer (nullable)
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(20);
            });

        // Configure Money value objects for OrderItem
        modelBuilder.Entity<OrderItem>()
            .OwnsOne(oi => oi.UnitPrice, money =>
            {
                money.Property(m => m.Amount).HasColumnName("UnitPrice_Amount");
                money.Property(m => m.Currency).HasColumnName("UnitPrice_Currency").HasMaxLength(3);
            });

        modelBuilder.Entity<OrderItem>()
            .OwnsOne(oi => oi.Discount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Discount_Amount");
                money.Property(m => m.Currency).HasColumnName("Discount_Currency").HasMaxLength(3);
            });

        // TotalPrice is a calculated property, no need to configure it

        // Payment entity uses decimal Amount and string Currency directly, no Money value object needed
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Don't configure anything here when using DbContext pooling
        // Configuration is handled in DI registration
    }
}