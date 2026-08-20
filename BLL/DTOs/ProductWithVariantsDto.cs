namespace BLL.DTOs;

/// <summary>
/// Product projection carrying its full list of active ProductVariants
/// nested underneath, for API consumers that need the Product → Variants
/// shape (one row per product, not one row per size).
/// </summary>
public sealed class ProductWithVariantsDto
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public int CategoryId { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public int TaxRateId { get; init; }

    public string TaxRateName { get; init; } = string.Empty;

    public decimal TaxRatePercentage { get; init; }

    public bool IsActive { get; init; }

    public List<ProductVariantDto> Variants { get; init; } = new();
}
