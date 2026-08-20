namespace BLL.DTOs;

public sealed class ModifierOptionTranslationDto
{
    public int ModifierOptionTranslationId { get; init; }

    public int ModifierOptionId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string TranslatedName { get; init; } = string.Empty;
}
