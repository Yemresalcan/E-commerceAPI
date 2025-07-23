using System.Linq.Expressions;
using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Domain.Specifications;

/// <summary>
/// Specification for orders by customer
/// </summary>
public class OrdersByCustomerSpecification : Specification<Order>
{
    private readonly Guid _customerId;

    public OrdersByCustomerSpecification(Guid customerId)
    {
        _customerId = customerId;
        ApplyOrderByDescending(order => order.CreatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.CustomerId == _customerId;
}

/// <summary>
/// Specification for orders by status
/// </summary>
public class OrdersByStatusSpecification : Specification<Order>
{
    private readonly OrderStatus _status;

    public OrdersByStatusSpecification(OrderStatus status)
    {
        _status = status;
        ApplyOrderByDescending(order => order.CreatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.Status == _status;
}

/// <summary>
/// Specification for orders by customer and status
/// </summary>
public class OrdersByCustomerAndStatusSpecification : Specification<Order>
{
    private readonly Guid _customerId;
    private readonly OrderStatus _status;

    public OrdersByCustomerAndStatusSpecification(Guid customerId, OrderStatus status)
    {
        _customerId = customerId;
        _status = status;
        ApplyOrderByDescending(order => order.CreatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.CustomerId == _customerId && order.Status == _status;
}

/// <summary>
/// Specification for orders within a date range
/// </summary>
public class OrdersByDateRangeSpecification : Specification<Order>
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public OrdersByDateRangeSpecification(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate.Date;
        _endDate = endDate.Date.AddDays(1).AddTicks(-1);
        ApplyOrderByDescending(order => order.CreatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.CreatedAt >= _startDate && order.CreatedAt <= _endDate;
}

/// <summary>
/// Specification for orders containing a specific product
/// </summary>
public class OrdersContainingProductSpecification : Specification<Order>
{
    private readonly Guid _productId;

    public OrdersContainingProductSpecification(Guid productId)
    {
        _productId = productId;
        ApplyOrderByDescending(order => order.CreatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.OrderItems.Any(item => item.ProductId == _productId);
}

/// <summary>
/// Specification for pending orders
/// </summary>
public class PendingOrdersSpecification : Specification<Order>
{
    public PendingOrdersSpecification()
    {
        ApplyOrderBy(order => order.CreatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.Status == OrderStatus.Pending;
}

/// <summary>
/// Specification for orders ready for shipping
/// </summary>
public class OrdersReadyForShippingSpecification : Specification<Order>
{
    public OrdersReadyForShippingSpecification()
    {
        ApplyOrderBy(order => order.CreatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.Status == OrderStatus.Confirmed;
}

/// <summary>
/// Specification for shipped orders
/// </summary>
public class ShippedOrdersSpecification : Specification<Order>
{
    public ShippedOrdersSpecification()
    {
        ApplyOrderByDescending(order => order.UpdatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.Status == OrderStatus.Shipped;
}

/// <summary>
/// Specification for orders with total amount above a threshold
/// </summary>
public class OrdersAboveAmountSpecification : Specification<Order>
{
    private readonly decimal _minimumAmount;
    private readonly string _currency;

    public OrdersAboveAmountSpecification(decimal minimumAmount, string currency)
    {
        _minimumAmount = minimumAmount;
        _currency = currency ?? throw new ArgumentNullException(nameof(currency));
        ApplyOrderByDescending(order => order.TotalAmount.Amount);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.TotalAmount.Amount >= _minimumAmount && 
                 order.TotalAmount.Currency == _currency;
}

/// <summary>
/// Specification for most recent order by customer
/// </summary>
public class MostRecentOrderByCustomerSpecification : Specification<Order>
{
    private readonly Guid _customerId;

    public MostRecentOrderByCustomerSpecification(Guid customerId)
    {
        _customerId = customerId;
        ApplyOrderByDescending(order => order.CreatedAt);
        ApplyPaging(0, 1);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.CustomerId == _customerId;
}

/// <summary>
/// Specification for paginated orders
/// </summary>
public class PaginatedOrdersSpecification : Specification<Order>
{
    public PaginatedOrdersSpecification(int pageNumber, int pageSize, bool orderByNewest = true)
    {
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        
        if (orderByNewest)
        {
            ApplyOrderByDescending(order => order.CreatedAt);
        }
        else
        {
            ApplyOrderBy(order => order.CreatedAt);
        }
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => true; // No filtering, just pagination and sorting
}

/// <summary>
/// Specification for paginated customer orders
/// </summary>
public class PaginatedCustomerOrdersSpecification : Specification<Order>
{
    private readonly Guid _customerId;

    public PaginatedCustomerOrdersSpecification(Guid customerId, int pageNumber, int pageSize)
    {
        _customerId = customerId;
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        ApplyOrderByDescending(order => order.CreatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => order.CustomerId == _customerId;
}

/// <summary>
/// Specification for orders by multiple statuses
/// </summary>
public class OrdersByMultipleStatusesSpecification : Specification<Order>
{
    private readonly IEnumerable<OrderStatus> _statuses;

    public OrdersByMultipleStatusesSpecification(IEnumerable<OrderStatus> statuses)
    {
        _statuses = statuses ?? throw new ArgumentNullException(nameof(statuses));
        ApplyOrderByDescending(order => order.CreatedAt);
    }

    public override Expression<Func<Order, bool>> Criteria => 
        order => _statuses.Contains(order.Status);
}