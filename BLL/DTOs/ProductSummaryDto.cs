namespace BLL.DTOs;

public sealed class ProductSummaryDto
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public int CategoryId { get; init; }

    /// <summary>
    /// Lowest active ProductVariant.UnitPrice for this product. Null when
    /// the product currently has no active variants.
    /// </summary>
    public decimal? MinUnitPrice { get; init; }

    /// <summary>
    /// Highest active ProductVariant.UnitPrice for this product. Equal to
    /// <see cref="MinUnitPrice"/> for single-size products.
    /// </summary>
    public decimal? MaxUnitPrice { get; init; }

    public int VariantCount { get; init; }

    public int TaxRateId { get; init; }

    public string TaxRateName { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}