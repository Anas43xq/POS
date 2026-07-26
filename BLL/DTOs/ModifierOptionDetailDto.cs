namespace BLL.DTOs;

/// <summary>
/// Full detail of a modifier option.
/// Used by the manager detail panel when viewing/editing options within a group.
/// </summary>
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