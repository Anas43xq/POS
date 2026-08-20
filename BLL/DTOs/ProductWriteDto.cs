using System.Collections.Generic;

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

    public int TaxRateId { get; init; }

    public bool IsActive { get; init; } = true;

    public string? Description { get; init; }

    /// <summary>
    /// The product's sellable Size/Price rows. This is the only source of
    /// selling price — Products no longer carry a price of their own.
    /// Rows with <see cref="ProductVariantWriteDto.VariantId"/> == 0 are
    /// inserted; existing ids are updated; ids present on the entity but
    /// missing from this list are deleted.
    /// </summary>
    public List<ProductVariantWriteDto> Variants { get; init; } = new();
}