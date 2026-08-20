using System.Collections.Generic;

namespace BLL.DTOs;

public sealed class ModifierGroupDto
{
    public int ModifierGroupId { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Canonical (English) name, regardless of the language the request
    /// was localized for. Used for persistence (transaction/receipt data)
    /// so those records stay in English even when the cashier UI is
    /// showing another language. Mirrors <c>ProductDto.EnglishName</c>.
    /// </summary>
    public string EnglishName { get; init; } = string.Empty;

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