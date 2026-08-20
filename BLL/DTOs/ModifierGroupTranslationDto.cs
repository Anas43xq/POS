namespace BLL.DTOs;

public sealed class ModifierGroupTranslationDto
{
    public int ModifierGroupTranslationId { get; init; }

    public int ModifierGroupId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string TranslatedName { get; init; } = string.Empty;
}
