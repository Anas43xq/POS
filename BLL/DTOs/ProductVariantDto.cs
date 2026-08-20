namespace BLL.DTOs;

public sealed class ProductVariantDto
{
    public int VariantId { get; init; }

    public int ProductId { get; init; }

    public int SizeId { get; init; }

    public string SizeName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public bool IsActive { get; init; } = true;
}