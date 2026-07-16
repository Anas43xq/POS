using System.Collections.Generic;

namespace BLL.DTOs;

/// <summary>
/// Read-only projection of a modifier group with its options.
/// Returned by <c>IModifierService</c>.
/// </summary>
public sealed class ModifierGroupDto
{
    public int ModifierGroupId { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 1 = SingleSelect, 2 = MultiSelect, 3 = Quantity
    /// </summary>
    public int GroupType { get; init; }

    public bool IsRequired { get; init; }

    public int MinSelections { get; init; }

    public int MaxSelections { get; init; }

    public int SortOrder { get; init; }

    public List<ModifierOptionDto> Options { get; init; } = new();
}