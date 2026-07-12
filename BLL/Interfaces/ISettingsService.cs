using Contracts.Shortcuts;
using POS.Contracts.Localization;
using POS.Contracts.Printing;

namespace BLL.Interfaces;

public interface ISettingsService
{
    Task SetLanguageAsync(LanguageCode languageCode);
    Task<LanguageCode> GetLanguageAsync();

    Task<PrinterSettings> GetPrinterSettingsAsync();
    Task SetPrinterSettingsAsync(PrinterSettings settings);

    Task<Dictionary<string, string>?> GetCustomShortcutBindingsAsync(string profileName);
    Task SetCustomShortcutBindingsAsync(string profileName, Dictionary<string, string> bindings);
    Task ClearCustomShortcutBindingsAsync(string profileName);
}