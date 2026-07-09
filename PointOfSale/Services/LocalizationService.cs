using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BLL.Interfaces;
using Microsoft.Extensions.Logging;
using POS.Contracts.Localization;

namespace UI.Services;

public sealed class LocalizationService : ILocalizationService
{
    private const string LocalizationPathPrefix = "Localization/";

    private readonly ISettingsService _settingsService;
    private readonly ILogger<LocalizationService> _logger;

    public LanguageDto CurrentLanguage { get; private set; } =
        SupportedLanguages.Defaultlanguage;

    /// <inheritdoc />
    public event EventHandler? LanguageChanged;

    public LocalizationService(
        ISettingsService settingsService,
        ILogger<LocalizationService> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <inheritdoc />
    public string GetString(string key, params object?[] args)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Resource key cannot be null or empty.", nameof(key));

        // TryFindResource walks the merged dictionaries, so it picks up
        // the currently-active language without the caller having to know
        // which file is loaded.
        object? resource = Application.Current?.TryFindResource(key);

        if (resource is not string format)
        {
            _logger.LogWarning("Missing localized string for key '{Key}'.", key);
            return $"<{key}>";
        }

        if (args is null || args.Length == 0)
            return format;

        try
        {
            return string.Format(format, args);
        }
        catch (FormatException ex)
        {
            // The active dictionary has a malformed format string (e.g.
            // {0} referenced but no args supplied, or vice versa). Return
            // the raw resource so the user sees something; log so the
            // translator / dev can fix it.
            _logger.LogWarning(ex,
                "Localized string '{Key}' could not be formatted with {ArgCount} arg(s).",
                key, args.Length);
            return format;
        }
    }

    /// <summary>
    /// Loads localization at app startup.
    /// <para>
    /// Strategy (deliberately defensive):
    /// </para>
    /// <list type="number">
    ///   <item><description>Always load <b>English</b> first as a guaranteed baseline,
    ///   so the app is never left without a working dictionary.</description></item>
    ///   <item><description>If a non-English saved language is found, try to load it.
    ///   If it succeeds, switch to it. If it fails (file missing, parse error, etc.),
    ///   log a warning and <b>stay on English</b>.</description></item>
    /// </list>
    /// </summary>
    public void Initialize()
    {
        // 1) English baseline — must succeed for the app to look right.
        if (!TryLoadLanguage(SupportedLanguages.Defaultlanguage))
        {
            _logger.LogError(
                "Failed to load English resource dictionary at startup. " +
                "The app will run with raw resource keys.");
            return;
        }

        CurrentLanguage = SupportedLanguages.Defaultlanguage;
        UpdateFlowDirection(SupportedLanguages.Defaultlanguage);

        // 2) Saved language override (if different from English).
        try
        {
            var saved = _settingsService.GetLanguageAsync().GetAwaiter().GetResult();
            var savedLanguage = SupportedLanguages.All
                .FirstOrDefault(l => l.Code == saved);

            if (savedLanguage is null || savedLanguage.Code == LanguageCode.English)
            {
                _logger.LogInformation("Startup language: English (default).");
                return;
            }

            if (TryLoadLanguage(savedLanguage))
            {
                UpdateFlowDirection(savedLanguage);
                CurrentLanguage = savedLanguage;
                _logger.LogInformation(
                    "Startup language loaded: {Language}.", savedLanguage.Code);
            }
            else
            {
                _logger.LogWarning(
                    "Saved language {Language} failed to load; staying on English.",
                    savedLanguage.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to read saved language; staying on English.");
        }
    }

    public IReadOnlyList<LanguageDto> GetSupportedLanguages()
        => SupportedLanguages.All;

    public async Task SetLanguageAsync(LanguageCode languageCode)
    {
        var language = SupportedLanguages.All
            .FirstOrDefault(l => l.Code == languageCode)
            ?? SupportedLanguages.Defaultlanguage;

        // No-op if already on this language.
        if (language.Code == CurrentLanguage.Code)
            return;

        // If the requested language fails to load, stay on the current one
        // (which is already a working dictionary).
        if (!TryLoadLanguage(language))
        {
            _logger.LogError(
                "Failed to load language {Language}; staying on {Current}.",
                language.Code, CurrentLanguage.Code);
            return;
        }

        UpdateFlowDirection(language);

        try
        {
            await _settingsService.SetLanguageAsync(language.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist language change to {Language}; UI updated but setting not saved.",
                language.Code);
        }

        CurrentLanguage = language;
        _logger.LogInformation(
            "Application language changed to {Language}.", language.Code);

        // Notify subscribers AFTER the dictionary has been swapped and
        // CurrentLanguage has been updated, so handlers that re-read
        // strings see the new language.
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Tries to load the given language's resource dictionary.
    /// Returns <c>true</c> on success, <c>false</c> if the file is missing
    /// or invalid (e.g. parse error). Replaces any previously-loaded
    /// localization dictionary in the application's merged dictionaries.
    /// </summary>
    private bool TryLoadLanguage(LanguageDto language)
    {
        try
        {
            LoadResourceDictionary(language);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to load resource dictionary for '{Prefix}'.",
                language.FilePrefix);
            return false;
        }
    }

    private static void LoadResourceDictionary(LanguageDto language)
    {
        var app = Application.Current
            ?? throw new InvalidOperationException(
                "Application.Current is null; cannot load language dictionary.");

        var localization = new ResourceDictionary
        {
            Source = new Uri(
                $"{LocalizationPathPrefix}{language.FilePrefix}.xaml",
                UriKind.Relative)
        };

        var dictionaries = app.Resources.MergedDictionaries;

        var existing = dictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains(LocalizationPathPrefix) == true);

        if (existing != null)
            dictionaries.Remove(existing);

        dictionaries.Add(localization);
    }

    private static void UpdateFlowDirection(LanguageDto language)
    {
        var mainWindow = Application.Current?.MainWindow;
        if (mainWindow == null)
            return;

        mainWindow.FlowDirection =
            language.IsRightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
    }
}
