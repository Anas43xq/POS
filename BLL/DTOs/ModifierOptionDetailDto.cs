namespace BLL.DTOs;

public sealed class ModifierOptionDetailDto
{
    public int ModifierOptionId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal PriceAdd { get; init; }

    public bool AllowQuantity { get; init; }

    public bool IsDefault { get; init; }

    public bool IsActive { get; init; }

    public int SortOrder { get; init; }
}