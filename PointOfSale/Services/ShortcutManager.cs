using System.Windows.Input;
using Contracts.Enum;
using Contracts.Shortcuts;
using Microsoft.Extensions.Logging;

namespace UI.Services;

public sealed class ShortcutManager
{
    private readonly IKeyboardShortcutService _shortcutService;
    private readonly ILogger<ShortcutManager>? _logger;

    public ShortcutManager(IKeyboardShortcutService shortcutService, ILogger<ShortcutManager>? logger)
    {
        _shortcutService = shortcutService;
        _logger = logger;

        _shortcutService.ShortcutTriggered += OnShortcutTriggered;
    }

    public event EventHandler<ShortcutAction>? ActionRequested;

    public bool ProcessKey(Key key, ModifierKeys modifiers)
    {
        if (_shortcutService.TryMatch(key, modifiers, out var action))
        {
            _logger?.LogDebug("Key matched: {Key}+{Modifiers} -> {Action}", key, modifiers, action);
            _shortcutService.Execute(action);
            return true;
        }

        return false;
    }

    public void SwitchProfile(ShortcutProfileType profile)
    {
        _shortcutService.SetProfile(profile);
    }

    public IReadOnlyList<ShortcutBinding> GetCurrentBindings()
    {
        return _shortcutService.GetActiveBindings();
    }

    public IReadOnlyList<ShortcutBinding> GetBindingsFor(ShortcutProfileType profile)
    {
        return _shortcutService.GetBindings(profile);
    }

    public ShortcutConflictResult ValidateProfile(ShortcutProfileType profile)
    {
        return _shortcutService.DetectConflicts(profile);
    }

    public void ResetProfile(ShortcutProfileType profile)
    {
        _shortcutService.ResetToDefaults(profile);
    }

    private void OnShortcutTriggered(object? sender, ShortcutAction action)
    {
        _logger?.LogDebug("Shortcut action triggered: {Action}", action);
        ActionRequested?.Invoke(this, action);
    }
}
