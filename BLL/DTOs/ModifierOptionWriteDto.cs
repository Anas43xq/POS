namespace BLL.DTOs;

/// <summary>
/// Write DTO for creating or updating a modifier option.
/// <see cref="ModifierOptionId"/> is null for create, non-null for update.
/// </summary>
public sealed class ModifierOptionWriteDto
{
    /// <summary>
    /// Null = new option (create). Non-null = update existing.
    /// </summary>
    public int? ModifierOptionId { get; init; }

    public int ModifierGroupId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal PriceAdd { get; init; }

    public bool AllowQuantity { get; init; }

    /// <summary>
    /// Whether this option is the default selection (e.g. "Regular Dough").
    /// For single-select groups, only one option should be default.
    /// </summary>
    public bool IsDefault { get; init; }

    public bool IsActive { get; init; } = true;

    public int SortOrder { get; init; }
}