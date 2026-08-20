namespace BLL.DTOs;

public sealed class ModifierOptionDto
{
    public int ModifierOptionId { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Canonical (English) name, regardless of the language the request
    /// was localized for. Used for persistence (transaction/receipt data)
    /// so those records stay in English even when the cashier UI is
    /// showing another language. Mirrors <c>ProductDto.EnglishName</c>.
    /// </summary>
    public string EnglishName { get; init; } = string.Empty;

    public decimal PriceAdd { get; init; }

    public bool AllowQuantity { get; init; }

    public bool IsDefault { get; init; }

    public int SortOrder { get; init; }
}