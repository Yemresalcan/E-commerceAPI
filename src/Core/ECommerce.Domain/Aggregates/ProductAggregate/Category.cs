using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Aggregates.ProductAggregate;

/// <summary>
/// Represents a product category with hierarchical structure support
/// </summary>
public class Category : AggregateRoot
{
    private readonly List<Category> _children = [];

    /// <summary>
    /// The name of the category
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The description of the category
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// The parent category identifier (null for root categories)
    /// </summary>
    public Guid? ParentCategoryId { get; private set; }

    /// <summary>
    /// The level in the hierarchy (0 for root categories)
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    /// Whether this category is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Read-only collection of child categories
    /// </summary>
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    /// <summary>
    /// Whether this category is a root category (has no parent)
    /// </summary>
    public bool IsRoot => ParentCategoryId == null;

    /// <summary>
    /// Whether this category has child categories
    /// </summary>
    public bool HasChildren => _children.Count > 0;

    // Private constructor for EF Core
    private Category() : base()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// Creates a new root category
    /// </summary>
    public static Category CreateRoot(string name, string description)
    {
        ValidateName(name);
        ValidateDescription(description);

        return new Category
        {
            Name = name,
            Description = description,
            ParentCategoryId = null,
            Level = 0,
            IsActive = true
        };
    }

    /// <summary>
    /// Creates a new child category under a parent
    /// </summary>
    public static Category CreateChild(string name, string description, Guid parentCategoryId, int parentLevel)
    {
        ValidateName(name);
        ValidateDescription(description);

        const int maxHierarchyLevel = 5; // Prevent too deep hierarchies
        if (parentLevel >= maxHierarchyLevel)
        {
            throw new InvalidCategoryHierarchyException($"Category hierarchy cannot exceed {maxHierarchyLevel} levels");
        }

        return new Category
        {
            Name = name,
            Description = description,
            ParentCategoryId = parentCategoryId,
            Level = parentLevel + 1,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the category name and description
    /// </summary>
    public void Update(string name, string description)
    {
        ValidateName(name);
        ValidateDescription(description);

        Name = name;
        Description = description;
        MarkAsModified();
    }

    /// <summary>
    /// Activates the category
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsModified();
    }

    /// <summary>
    /// Deactivates the category
    /// </summary>
    public void Deactivate()
    {
        if (HasChildren)
        {
            throw new InvalidCategoryHierarchyException("Cannot deactivate a category that has active child categories");
        }

        IsActive = false;
        MarkAsModified();
    }

    /// <summary>
    /// Adds a child category to this category
    /// </summary>
    public void AddChild(Category child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (child.ParentCategoryId != Id)
        {
            throw new InvalidCategoryHierarchyException("Child category's parent ID must match this category's ID");
        }

        if (_children.Any(c => c.Id == child.Id))
        {
            throw new InvalidCategoryHierarchyException("Child category already exists");
        }

        _children.Add(child);
        MarkAsModified();
    }

    /// <summary>
    /// Removes a child category from this category
    /// </summary>
    public void RemoveChild(Category child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (_children.Remove(child))
        {
            MarkAsModified();
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be null or empty", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Category name cannot exceed 100 characters", nameof(name));
        }
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Category description cannot be null or empty", nameof(description));
        }

        if (description.Length > 500)
        {
            throw new ArgumentException("Category description cannot exceed 500 characters", nameof(description));
        }
    }
}