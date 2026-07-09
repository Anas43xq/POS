using POS.Contracts.Localization;

namespace BLL.Interfaces;

public interface ILocalizationService
{
    /// <summary>
    /// Gets the currently selected application language.
    /// </summary>
    LanguageDto CurrentLanguage { get; }

    /// <summary>
    /// Loads the saved language. Must be called after <see cref="System.Windows.Application.MainWindow"/>
    /// is set so the FlowDirection can be applied.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Changes the application language.
    /// </summary>
    Task SetLanguageAsync(LanguageCode languageCode);

    /// <summary>
    /// Gets all supported application languages.
    /// </summary>
    IReadOnlyList<LanguageDto> GetSupportedLanguages();

    /// <summary>
    /// Raised after the active language has been successfully switched
    /// and the active resource dictionary has been replaced. UI
    /// subscribers that expose cached localized strings (e.g. a
    /// computed display string built via <see cref="GetString"/>)
    /// should re-read them and raise <c>PropertyChanged</c> so the
    /// View re-evaluates the bound property.
    /// </summary>
    event EventHandler? LanguageChanged;

    /// <summary>
    /// Resolves a localized string by key and applies
    /// <see cref="string.Format(string, object?[])"/> with the
    /// supplied arguments. Used for format strings like
    /// <c>"{0} items"</c> that need substitution at runtime.
    /// <para>
    /// If the key is missing from the active dictionary, the key is
    /// returned in angle brackets (e.g. <c><Common.ItemsCount></c>)
    /// so missing-key bugs are visible instead of blank. If the
    /// resolved format string is malformed, the raw resource is
    /// returned and the issue is logged.
    /// </para>
    /// </summary>
    string GetString(string key, params object?[] args);
}
