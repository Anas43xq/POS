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

    public Task<Dictionary<string, string>?> GetCustomShortcutBindingsAsync(string profileName)
    {
        try
        {
            var settings = LoadSettings();
            if (settings.ShortcutBindings != null
                && settings.ShortcutBindings.TryGetValue(profileName, out var bindings))
            {
                return Task.FromResult<Dictionary<string, string>?>(bindings);
            }
            return Task.FromResult<Dictionary<string, string>?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read shortcut bindings for profile {Profile}.", profileName);
            return Task.FromResult<Dictionary<string, string>?>(null);
        }
    }

    public Task SetCustomShortcutBindingsAsync(string profileName, Dictionary<string, string> bindings)
    {
        try
        {
            var settings = LoadSettings();
            settings.ShortcutBindings ??= new Dictionary<string, Dictionary<string, string>>();
            settings.ShortcutBindings[profileName] = bindings;
            SaveSettings(settings);

            _logger.LogInformation("Shortcut bindings saved for profile {Profile}.", profileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist shortcut bindings for profile {Profile}.", profileName);
        }

        return Task.CompletedTask;
    }

    public Task ClearCustomShortcutBindingsAsync(string profileName)
    {
        try
        {
            var settings = LoadSettings();
            if (settings.ShortcutBindings != null)
            {
                settings.ShortcutBindings.Remove(profileName);
                SaveSettings(settings);
            }

            _logger.LogInformation("Custom shortcut bindings cleared for profile {Profile}.", profileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear shortcut bindings for profile {Profile}.", profileName);
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
        public Dictionary<string, Dictionary<string, string>>? ShortcutBindings { get; set; }
    }
}