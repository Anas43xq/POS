using Contracts.Enum;
using Contracts.Shortcuts;
using System.Windows.Input;

namespace UI.Services;

public interface IKeyboardShortcutService
{
    ShortcutProfileType ActiveProfile { get; }

    event EventHandler<ShortcutAction>? ShortcutTriggered;

    void SetProfile(ShortcutProfileType profile);

    void Register(ShortcutProfile profile);

    void RegisterBinding(ShortcutProfileType profile, ShortcutBinding binding);

    void Unregister(ShortcutProfileType profile, ShortcutAction action);

    IReadOnlyList<ShortcutBinding> GetBindings(ShortcutProfileType profile);

    IReadOnlyList<ShortcutBinding> GetActiveBindings();

    ShortcutConflictResult DetectConflicts(ShortcutProfileType profile);

    void ResetToDefaults(ShortcutProfileType profile);

    void Execute(ShortcutAction action);

    bool TryMatch(Key key, ModifierKeys modifiers, out ShortcutAction action);

    Task LoadCustomBindingsAsync(ShortcutProfileType profile);

    Task SaveCustomBindingsAsync(ShortcutProfileType profile);

    Task ClearCustomBindingsAsync(ShortcutProfileType profile);
}
