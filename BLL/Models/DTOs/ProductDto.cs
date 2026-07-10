namespace BLL.DTOs;

public sealed class ProductDto
{
    public int ProductId { get; init; }

    public int VariantId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public decimal TaxRate { get; init; }

    public int CategoryId { get; init; }

    public bool IsActive { get; init; }
}