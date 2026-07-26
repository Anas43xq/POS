namespace BLL.DTOs;

/// <summary>
/// Write DTO for creating or updating a modifier group.
/// <see cref="ModifierGroupId"/> is null for create, non-null for update.
/// </summary>
public sealed class ModifierGroupWriteDto
{
    /// <summary>
    /// Null = new group (create). Non-null = update existing.
    /// </summary>
    public int? ModifierGroupId { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 1 = SingleSelect, 2 = MultiSelect, 3 = Quantity
    /// </summary>
    public byte GroupType { get; init; }

    public bool IsRequired { get; init; }

    public bool IsActive { get; init; } = true;

    public int MinSelections { get; init; }

    public int MaxSelections { get; init; }

    public int SortOrder { get; init; }
}