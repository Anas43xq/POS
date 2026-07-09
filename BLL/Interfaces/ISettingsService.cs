using POS.Contracts.Localization;

namespace BLL.Interfaces;

public interface ISettingsService
{
    /// <summary>
    /// Persists the selected language code.
    /// </summary>
    Task SetLanguageAsync(LanguageCode languageCode);

    /// <summary>
    /// Retrieves the persisted language code, or <see cref="LanguageCode.English"/> if none is stored.
    /// </summary>
    Task<LanguageCode> GetLanguageAsync();
}