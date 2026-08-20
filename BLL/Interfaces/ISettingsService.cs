using POS.Contracts.Localization;
using POS.Contracts.Printing;

namespace BLL.Interfaces;

public interface ISettingsService
{
    Task SetLanguageAsync(LanguageCode languageCode);
    Task<LanguageCode> GetLanguageAsync();

    Task<PrinterSettings> GetPrinterSettingsAsync();
    Task SetPrinterSettingsAsync(PrinterSettings settings);
}