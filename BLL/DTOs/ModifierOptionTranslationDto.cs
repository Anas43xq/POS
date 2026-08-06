namespace BLL.DTOs;

/// <summary>
/// Localized name for a modifier option.
/// </summary>
public sealed class ModifierOptionTranslationDto
{
    public int ModifierOptionTranslationId { get; init; }

    public int ModifierOptionId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string TranslatedName { get; init; } = string.Empty;
}
