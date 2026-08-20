namespace BLL.DTOs;

/// <summary>
/// Write model for a single Size/Price row edited on the Product form.
/// A list of these is carried on <see cref="ProductWriteDto"/> so a
/// product and all of its variants can be saved together.
/// </summary>
public sealed class ProductVariantWriteDto
{
    /// <summary>
    /// Zero for a new variant; the existing Id when editing one that
    /// already exists on the product.
    /// </summary>
    public int VariantId { get; init; }

    public int SizeId { get; init; }

    public string SizeName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public bool IsActive { get; init; } = true;
}
