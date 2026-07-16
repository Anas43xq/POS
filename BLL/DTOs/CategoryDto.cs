namespace BLL.DTOs;

/// <summary>
/// Read-only projection of a category, including its children and product count.
/// Returned by <c>ICategoryService</c>.
/// </summary>
public sealed class CategoryDto
{
    public int CategoryId { get; init; }

    public string Name { get; init; } = string.Empty;

    public int? ParentCategoryId { get; init; }

    public string? Description { get; init; }

    public List<CategoryDto> ChildCategories { get; init; } = new();

    public int ProductCount { get; init; }

    /// <summary>
    /// UI-only flag indicating whether this category is currently selected
    /// in the cashier sidebar. Transient — never persisted or serialized.
    /// </summary>
    public bool IsSelected { get; set; }
}
