namespace POS.Contracts.Localization;

public static class SupportedLanguages
{
    public static IReadOnlyList<LanguageDto> All { get; } =
    [
         new()
        {
            Code = LanguageCode.English,
            DisplayName = "English",
            IsRightToLeft = false
        },
        new()
        {
            Code = LanguageCode.Arabic,
            DisplayName = "العربية",
            IsRightToLeft = true
        },
        new()
        {
            Code = LanguageCode.Malayalam,
            DisplayName = "മലയാളം",
            IsRightToLeft = false
        }
       
    ];

    public static LanguageDto Defaultlanguage => All[0]; // LanguageCode.English
}