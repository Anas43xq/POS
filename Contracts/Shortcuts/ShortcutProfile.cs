using Contracts.Enum;

namespace Contracts.Shortcuts;

public sealed class ShortcutProfile
{
    public required ShortcutProfileType ProfileType { get; init; }

    public List<ShortcutBinding> Bindings { get; set; } = [];
}
