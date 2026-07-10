namespace BLL.DTOs;

/// <summary>
/// Localized name for a product.
/// </summary>
public sealed class ProductTranslationDto
{
    public int ProductTranslationId { get; init; }

    public int ProductId { get; init; }

    public string LanguageCode { get; init; } = string.Empty;

    public string TranslatedName { get; init; } = string.Empty;
}