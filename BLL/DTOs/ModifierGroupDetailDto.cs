namespace BLL.DTOs;

/// <summary>
/// Full detail of a modifier group including its options.
/// Used by the manager detail panel.
/// </summary>
public sealed class ModifierGroupDetailDto
{
    public int ModifierGroupId { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 1 = SingleSelect, 2 = MultiSelect, 3 = Quantity
    /// </summary>
    public int GroupType { get; init; }

    public bool IsRequired { get; init; }

    public bool IsActive { get; init; }

    public int MinSelections { get; init; }

    public int MaxSelections { get; init; }

    public int SortOrder { get; init; }

    public List<ModifierOptionDetailDto> Options { get; init; } = new();
}