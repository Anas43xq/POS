using BLL.Interfaces;
using Microsoft.Extensions.Logging;
using POS.Contracts.Localization;
using POS.Contracts.Printing;
using System.Text.Json;

namespace UI.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private static readonly string SettingsFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "usersettings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
    }

    public Task SetLanguageAsync(LanguageCode languageCode)
    {
        try
        {
            var settings = LoadSettings();
            settings.LanguageCode = languageCode;
            SaveSettings(settings);

            _logger.LogInformation("Language setting saved: {Language}", languageCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist language setting.");
        }

        return Task.CompletedTask;
    }

    public Task<LanguageCode> GetLanguageAsync()
    {
        try
        {
            var settings = LoadSettings();
            return Task.FromResult(settings.LanguageCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read language setting; defaulting to English.");
            return Task.FromResult(LanguageCode.English);
        }
    }

    public Task<PrinterSettings> GetPrinterSettingsAsync()
    {
        try
        {
            var settings = LoadSettings();
            return Task.FromResult(settings.Printer ?? new PrinterSettings());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read printer settings; using defaults.");
            return Task.FromResult(new PrinterSettings());
        }
    }

    public Task SetPrinterSettingsAsync(PrinterSettings settings)
    {
        try
        {
            var userSettings = LoadSettings();
            userSettings.Printer = settings;
            SaveSettings(userSettings);

            _logger.LogInformation("Printer settings saved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist printer settings.");
        }

        return Task.CompletedTask;
    }

    private static UserSettings LoadSettings()
    {
        if (!File.Exists(SettingsFilePath))
            return new UserSettings();

        try
        {
            var json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
        }
        catch
        {
            return new UserSettings();
        }
    }

    private static void SaveSettings(UserSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsFilePath, json);
    }

    private sealed class UserSettings
    {
        public LanguageCode LanguageCode { get; set; } = LanguageCode.English;
        public PrinterSettings? Printer { get; set; }
    }
}