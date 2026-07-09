namespace POS.Contracts.Localization;

public sealed class LanguageDto
{
    public required LanguageCode Code { get; init; }

    public required string DisplayName { get; init; }

    public bool IsRightToLeft { get; init; }

    public bool IsDefault { get; init; }
}