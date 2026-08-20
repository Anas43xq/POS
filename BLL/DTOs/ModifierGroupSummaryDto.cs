namespace BLL.DTOs;

/// <summary>
/// Lightweight projection of a modifier group for list views.
/// Does not include options — use <see cref="ModifierGroupDetailDto"/> for detail.
/// </summary>
public sealed class ModifierGroupSummaryDto
{
    public int ModifierGroupId { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 1 = SingleSelect, 2 = MultiSelect, 3 = Quantity
    /// </summary>
    public int GroupType { get; init; }

    /// <summary>
    /// Localized display string for <see cref="GroupType"/>.
    /// </summary>
    public string GroupTypeDisplay { get; init; } = string.Empty;

    public bool IsRequired { get; init; }

    public bool IsActive { get; init; }

    public int OptionCount { get; init; }

    public int SortOrder { get; init; }
}