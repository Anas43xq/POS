namespace BLL.DTOs;

/// <summary>
/// Read-only projection of a modifier option.
/// Returned as part of <c>ModifierGroupDto</c>.
/// </summary>
public sealed class ModifierOptionDto
{
    public int ModifierOptionId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal PriceAdd { get; init; }

    public bool AllowQuantity { get; init; }

    public bool IsDefault { get; init; }

    public int SortOrder { get; init; }
}