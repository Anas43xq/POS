namespace BLL.DTOs;

/// <summary>
/// Lightweight product projection used for product management and report filters.
/// </summary>
public sealed class ProductSummaryDto
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public int CategoryId { get; init; }

    public decimal UnitPrice { get; init; }

    public int TaxRateId { get; init; }

    public string TaxRateName { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}