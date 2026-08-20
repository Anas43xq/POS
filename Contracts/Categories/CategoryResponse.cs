using System.Collections.Generic;

namespace Contracts.Categories
{
    /// <summary>
    /// API response shape for a single category: one row per category,
    /// carrying its subcategories nested underneath. ProductCount is a
    /// rollup that already includes child categories' product counts.
    /// </summary>
    public class CategoryResponse
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryResponse> ChildCategories { get; set; } = new();
    }
}
