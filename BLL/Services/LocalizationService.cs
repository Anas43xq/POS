using BLL.Interfaces;
using Microsoft.Extensions.Logging;
using POS.Contracts.Localization;
using System.Globalization;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace UI.Services;

public sealed class LocalizationService : ILocalizationService
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<LocalizationService> _logger;

    public LanguageDto CurrentLanguage { get; private set; }

    public LocalizationService(
        ISettingsService settingsService,
        ILogger<LocalizationService> logger)
    {
        _settingsService = settingsService;
        _logger = logger;

        CurrentLanguage = SupportedLanguages.Default;
    }

    public IReadOnlyList<LanguageDto> GetSupportedLanguages()
        => SupportedLanguages.All;

    public async Task SetLanguageAsync(LanguageCode languageCode)
    {
        var language = SupportedLanguages.All
            .FirstOrDefault(l => l.Code == languageCode)
            ?? SupportedLanguages.Default;

        if (language.Code == CurrentLanguage.Code)
            return;

        LoadResourceDictionary(language);

        UpdateCulture(language);

        UpdateFlowDirection(language);

        await _settingsService.SetLanguageAsync(language.Code);

        CurrentLanguage = language;

        _logger.LogInformation("Application language changed to {Language}.", language.Code);
    }

    private static void LoadResourceDictionary(LanguageDto language)
    {
        var app = Application.Current;

        var localization = new ResourceDictionary
        {
            Source = new Uri(
                $"Localization/{language.Code.ToString().ToLower()}.xaml",
                UriKind.Relative)
        };

        var dictionaries = app.Resources.MergedDictionaries;

        var existing = dictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Localization/") == true);

        if (existing != null)
            dictionaries.Remove(existing);

        dictionaries.Add(localization);
    }

    private static void UpdateCulture(LanguageDto language)
    {
        var culture = language.Code switch
        {
            LanguageCode.Arabic => new CultureInfo("ar"),
            LanguageCode.Malayalam => new CultureInfo("ml"),
            _ => new CultureInfo("en")
        };

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    private static void UpdateFlowDirection(LanguageDto language)
    {
        Application.Current.MainWindow!.FlowDirection =
            language.IsRightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
    }
}