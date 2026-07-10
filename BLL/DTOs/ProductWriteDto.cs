namespace BLL.DTOs;

/// <summary>
/// Write model for creating or updating a product.
/// Passed from the UI to <c>IProductService</c>.
/// </summary>
public sealed class ProductWriteDto
{
    /// <summary>
    /// Zero for new products; the existing Id for updates.
    /// </summary>
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public int CategoryId { get; init; }

    public decimal UnitPrice { get; init; }

    public int TaxRateId { get; init; }

    public bool IsActive { get; init; } = true;

    public string? Description { get; init; }
}