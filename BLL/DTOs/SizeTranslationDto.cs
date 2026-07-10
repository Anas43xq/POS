namespace BLL.DTOs;

/// <summary>
/// Localized name for a size.
/// </summary>
public sealed class SizeTranslationDto
{
    public int SizeTranslationId { get; init; }

    public int SizeId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string TranslatedName { get; init; } = string.Empty;
}