using BLL.Interfaces;
using Contracts.Enum;
using Contracts.Shortcuts;
using Microsoft.Extensions.Logging;
using System.Windows.Input;

namespace UI.Services;

public sealed class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly ILogger<KeyboardShortcutService>? _logger;
    private readonly ISettingsService _settingsService;
    private readonly Dictionary<ShortcutProfileType, List<ShortcutBinding>> _profiles = new();
    private ShortcutProfileType _activeProfile = ShortcutProfileType.Cashier;

    public event EventHandler<ShortcutAction>? ShortcutTriggered;

    public ShortcutProfileType ActiveProfile => _activeProfile;

    public KeyboardShortcutService(ILogger<KeyboardShortcutService>? logger, ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
        LoadDefaults();
    }

    public void SetProfile(ShortcutProfileType profile)
    {
        _activeProfile = profile;
        _logger?.LogDebug("Keyboard shortcut profile changed to {Profile}", profile);
    }

    public void Register(ShortcutProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        _profiles[profile.ProfileType] = [.. profile.Bindings];
        _logger?.LogDebug("Registered {Count} bindings for profile {Profile}", profile.Bindings.Count, profile.ProfileType);
    }

    public void RegisterBinding(ShortcutProfileType profile, ShortcutBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        if (!_profiles.TryGetValue(profile, out var bindings))
        {
            bindings = [];
            _profiles[profile] = bindings;
        }

        var existing = bindings.FindIndex(b => b.Action == binding.Action);
        if (existing >= 0)
            bindings[existing] = binding;
        else
            bindings.Add(binding);
    }

    public void Unregister(ShortcutProfileType profile, ShortcutAction action)
    {
        if (_profiles.TryGetValue(profile, out var bindings))
            bindings.RemoveAll(b => b.Action == action);
    }

    public IReadOnlyList<ShortcutBinding> GetBindings(ShortcutProfileType profile)
    {
        return _profiles.TryGetValue(profile, out var bindings)
            ? bindings.AsReadOnly()
            : [];
    }

    public IReadOnlyList<ShortcutBinding> GetActiveBindings()
    {
        return GetBindings(_activeProfile);
    }

    public ShortcutConflictResult DetectConflicts(ShortcutProfileType profile)
    {
        var bindings = GetBindings(profile);
        var result = new ShortcutConflictResult();

        var byGesture = bindings
            .Where(b => b.IsEnabled)
            .GroupBy(b => b.KeyGesture.ToUpperInvariant());

        foreach (var group in byGesture)
        {
            var items = group.ToList();
            if (items.Count <= 1)
                continue;

            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    result.Conflicts.Add(new ShortcutConflict
                    {
                        ExistingAction = items[i].Action,
                        ExistingGesture = items[i].KeyGesture,
                        ConflictingAction = items[j].Action,
                        ConflictingGesture = items[j].KeyGesture
                    });
                }
            }
        }

        return result;
    }

    public void ResetToDefaults(ShortcutProfileType profile)
    {
        var defaults = DefaultShortcutProfiles.GetDefault(profile);
        _profiles[profile] = [.. defaults.Bindings];
        _logger?.LogDebug("Reset profile {Profile} to defaults", profile);
    }

    public void Execute(ShortcutAction action)
    {
        _logger?.LogDebug("Executing shortcut action {Action} on profile {Profile}", action, _activeProfile);
        ShortcutTriggered?.Invoke(this, action);
    }

    public bool TryMatch(Key key, ModifierKeys modifiers, out ShortcutAction action)
    {
        action = default;

        var bindings = GetActiveBindings();
        foreach (var binding in bindings)
        {
            if (!binding.IsEnabled)
                continue;

            if (!KeyGestureParser.TryParse(binding.KeyGesture, out var parsedKey, out var parsedModifiers))
                continue;

            if (parsedKey == key && parsedModifiers == modifiers)
            {
                action = binding.Action;
                return true;
            }
        }

        return false;
    }

    public async Task LoadCustomBindingsAsync(ShortcutProfileType profile)
    {
        try
        {
            var customBindings = await _settingsService.GetCustomShortcutBindingsAsync(profile.ToString());
            if (customBindings == null || customBindings.Count == 0)
                return;

            var defaults = DefaultShortcutProfiles.GetDefault(profile);
            foreach (var binding in defaults.Bindings)
            {
                if (customBindings.TryGetValue(binding.Action.ToString(), out var gesture))
                {
                    var updated = new ShortcutBinding
                    {
                        Action = binding.Action,
                        KeyGesture = gesture,
                        Description = binding.Description,
                        IsEnabled = binding.IsEnabled
                    };
                    RegisterBinding(profile, updated);
                }
            }

            _logger?.LogDebug("Loaded {Count} custom bindings for profile {Profile}", customBindings.Count, profile);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load custom bindings for profile {Profile}", profile);
        }
    }

    public async Task SaveCustomBindingsAsync(ShortcutProfileType profile)
    {
        try
        {
            var defaults = DefaultShortcutProfiles.GetDefault(profile);
            var current = GetBindings(profile);
            var customBindings = new Dictionary<string, string>();

            foreach (var binding in current)
            {
                var defaultBinding = defaults.Bindings.FirstOrDefault(b => b.Action == binding.Action);
                if (defaultBinding != null && !string.Equals(defaultBinding.KeyGesture, binding.KeyGesture, StringComparison.OrdinalIgnoreCase))
                {
                    customBindings[binding.Action.ToString()] = binding.KeyGesture;
                }
            }

            if (customBindings.Count > 0)
                await _settingsService.SetCustomShortcutBindingsAsync(profile.ToString(), customBindings);
            else
                await _settingsService.ClearCustomShortcutBindingsAsync(profile.ToString());

            _logger?.LogDebug("Saved {Count} custom bindings for profile {Profile}", customBindings.Count, profile);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save custom bindings for profile {Profile}", profile);
        }
    }

    public async Task ClearCustomBindingsAsync(ShortcutProfileType profile)
    {
        try
        {
            await _settingsService.ClearCustomShortcutBindingsAsync(profile.ToString());
            ResetToDefaults(profile);
            _logger?.LogDebug("Cleared custom bindings for profile {Profile}", profile);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear custom bindings for profile {Profile}", profile);
        }
    }

    private void LoadDefaults()
    {
        _profiles[ShortcutProfileType.Cashier] = [.. DefaultShortcutProfiles.GetCashierProfile().Bindings];
        _profiles[ShortcutProfileType.Manager] = [.. DefaultShortcutProfiles.GetManagerProfile().Bindings];
    }
}
