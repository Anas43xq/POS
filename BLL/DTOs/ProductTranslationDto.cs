namespace BLL.DTOs;

public sealed class ProductTranslationDto
{
    public int ProductTranslationId { get; init; }

    public int ProductId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string TranslatedName { get; init; } = string.Empty;
}