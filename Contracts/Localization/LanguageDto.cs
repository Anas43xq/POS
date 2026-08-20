namespace POS.Contracts.Localization;

public sealed class LanguageDto
{
    public required LanguageCode Code { get; init; }

    public required string DisplayName { get; init; }

    /// <summary>
    /// Short culture code used to locate the matching ResourceDictionary file
    /// under <c>Localization/{FilePrefix}.xaml</c> (e.g. "en", "ar", "ml").
    /// </summary>
    public required string FilePrefix { get; init; }

    public bool IsRightToLeft { get; init; }

    public bool IsDefault { get; init; }
}
