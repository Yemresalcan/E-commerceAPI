using System.Linq.Expressions;
using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Specifications;

/// <summary>
/// Specification for customer by email
/// </summary>
public class CustomerByEmailSpecification : Specification<Customer>
{
    private readonly string _email;

    public CustomerByEmailSpecification(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);
        _email = email.Value;
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.Email.Value == _email;
}

/// <summary>
/// Specification for customer by phone number
/// </summary>
public class CustomerByPhoneNumberSpecification : Specification<Customer>
{
    private readonly string _phoneNumber;

    public CustomerByPhoneNumberSpecification(PhoneNumber phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        _phoneNumber = phoneNumber.Value;
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.PhoneNumber != null && customer.PhoneNumber.Value == _phoneNumber;
}

/// <summary>
/// Specification for active customers
/// </summary>
public class ActiveCustomersSpecification : Specification<Customer>
{
    public ActiveCustomersSpecification()
    {
        ApplyOrderBy(customer => customer.LastName);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.IsActive;
}

/// <summary>
/// Specification for inactive customers
/// </summary>
public class InactiveCustomersSpecification : Specification<Customer>
{
    public InactiveCustomersSpecification()
    {
        ApplyOrderBy(customer => customer.LastName);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => !customer.IsActive;
}

/// <summary>
/// Specification for customers registered within a date range
/// </summary>
public class CustomersByRegistrationDateRangeSpecification : Specification<Customer>
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public CustomersByRegistrationDateRangeSpecification(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate.Date;
        _endDate = endDate.Date.AddDays(1).AddTicks(-1);
        ApplyOrderByDescending(customer => customer.RegistrationDate);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.RegistrationDate >= _startDate && customer.RegistrationDate <= _endDate;
}

/// <summary>
/// Specification for customers by last active date range
/// </summary>
public class CustomersByLastActiveDateRangeSpecification : Specification<Customer>
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public CustomersByLastActiveDateRangeSpecification(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate.Date;
        _endDate = endDate.Date.AddDays(1).AddTicks(-1);
        ApplyOrderByDescending(customer => customer.LastActiveDate ?? DateTime.MinValue);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.LastActiveDate.HasValue && 
                    customer.LastActiveDate.Value >= _startDate && 
                    customer.LastActiveDate.Value <= _endDate;
}

/// <summary>
/// Specification for searching customers by name
/// </summary>
public class CustomerSearchByNameSpecification : Specification<Customer>
{
    private readonly string _searchTerm;

    public CustomerSearchByNameSpecification(string searchTerm)
    {
        _searchTerm = searchTerm?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(searchTerm));
        ApplyOrderBy(customer => customer.LastName);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.FirstName.ToLower().Contains(_searchTerm) || 
                    customer.LastName.ToLower().Contains(_searchTerm);
}

/// <summary>
/// Specification for customers by multiple IDs
/// </summary>
public class CustomersByIdsSpecification : Specification<Customer>
{
    private readonly IEnumerable<Guid> _customerIds;

    public CustomersByIdsSpecification(IEnumerable<Guid> customerIds)
    {
        _customerIds = customerIds ?? throw new ArgumentNullException(nameof(customerIds));
        ApplyOrderBy(customer => customer.LastName);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => _customerIds.Contains(customer.Id);
}

/// <summary>
/// Specification for customers who haven't been active since a cutoff date
/// </summary>
public class InactiveCustomersSinceSpecification : Specification<Customer>
{
    private readonly DateTime _cutoffDate;

    public InactiveCustomersSinceSpecification(DateTime cutoffDate)
    {
        _cutoffDate = cutoffDate;
        ApplyOrderBy(customer => customer.LastActiveDate ?? DateTime.MinValue);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => !customer.LastActiveDate.HasValue || customer.LastActiveDate.Value < _cutoffDate;
}

/// <summary>
/// Specification for paginated customers
/// </summary>
public class PaginatedCustomersSpecification : Specification<Customer>
{
    public PaginatedCustomersSpecification(int pageNumber, int pageSize, bool orderByName = true)
    {
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        
        if (orderByName)
        {
            ApplyOrderBy(customer => customer.LastName);
        }
        else
        {
            ApplyOrderByDescending(customer => customer.RegistrationDate);
        }
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => true; // No filtering, just pagination and sorting
}

/// <summary>
/// Specification for customers with email verification status
/// </summary>
public class CustomersByEmailVerificationSpecification : Specification<Customer>
{
    private readonly bool _isEmailVerified;

    public CustomersByEmailVerificationSpecification(bool isEmailVerified)
    {
        _isEmailVerified = isEmailVerified;
        ApplyOrderBy(customer => customer.LastName);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.Profile.IsVerified == _isEmailVerified;
}

/// <summary>
/// Specification for customers who have opted in for marketing
/// </summary>
public class CustomersOptedInForMarketingSpecification : Specification<Customer>
{
    public CustomersOptedInForMarketingSpecification()
    {
        ApplyOrderBy(customer => customer.LastName);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.IsActive && customer.Profile.ReceiveMarketingEmails;
}

/// <summary>
/// Specification for customers by preferred language
/// </summary>
public class CustomersByPreferredLanguageSpecification : Specification<Customer>
{
    private readonly string _language;

    public CustomersByPreferredLanguageSpecification(string language)
    {
        _language = language ?? throw new ArgumentNullException(nameof(language));
        ApplyOrderBy(customer => customer.LastName);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.Profile.PreferredLanguage == _language;
}

/// <summary>
/// Specification for customers by preferred currency
/// </summary>
public class CustomersByPreferredCurrencySpecification : Specification<Customer>
{
    private readonly string _currency;

    public CustomersByPreferredCurrencySpecification(string currency)
    {
        _currency = currency ?? throw new ArgumentNullException(nameof(currency));
        ApplyOrderBy(customer => customer.LastName);
    }

    public override Expression<Func<Customer, bool>> Criteria => 
        customer => customer.Profile.PreferredCurrency.ToString() == _currency;
}