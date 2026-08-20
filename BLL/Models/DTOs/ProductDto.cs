namespace BLL.DTOs;

public sealed class ProductDto
{
    public int ProductId { get; init; }

    public int VariantId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Always-English product name, regardless of the active language.
    /// Used for receipt snapshots stored in <c>TransactionItems.ProductName</c>.
    /// </summary>
    public string EnglishName { get; init; } = string.Empty;

    /// <summary>
    /// Always-English formatted display name (via <see cref="BLL.Helpers.ProductNameFormatter"/>),
    /// regardless of the active language. Used for receipt snapshots.
    /// </summary>
    public string EnglishDisplayName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public decimal TaxRate { get; init; }

    public int CategoryId { get; init; }

    public bool IsActive { get; init; }

    /// <summary>
    /// Whether this product has modifier groups assigned
    /// (via ProductModifierGroups or CategoryModifierGroups join tables).
    /// Computed during product loading — never persisted standalone.
    /// </summary>
    public bool HasModifiers { get; init; }
}