using POS.Contracts.Localization;

namespace BLL.Interfaces;

public interface ILocalizationService
{
    /// <summary>
    /// Gets the currently selected application language.
    /// </summary>
    LanguageDto CurrentLanguage { get; }

    /// <summary>
    /// Changes the application language.
    /// </summary>
    Task SetLanguageAsync(LanguageCode languageCode);

    /// <summary>
    /// Gets all supported application languages.
    /// </summary>
    IReadOnlyList<LanguageDto> GetSupportedLanguages();
}