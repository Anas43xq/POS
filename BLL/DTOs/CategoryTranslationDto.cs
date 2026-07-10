namespace BLL.DTOs;

/// <summary>
/// Localized name for a category.
/// </summary>
public sealed class CategoryTranslationDto
{
    public int CategoryTranslationId { get; init; }

    public int CategoryId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string TranslatedName { get; init; } = string.Empty;
}