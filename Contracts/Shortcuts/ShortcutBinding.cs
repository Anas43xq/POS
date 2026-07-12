using Contracts.Enum;

namespace Contracts.Shortcuts;

public sealed class ShortcutBinding
{
    public required ShortcutAction Action { get; init; }

    public required string KeyGesture { get; init; }

    public string Description { get; init; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public override bool Equals(object? obj)
    {
        return obj is ShortcutBinding other
            && other.Action == Action
            && string.Equals(other.KeyGesture, KeyGesture, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Action, KeyGesture.ToUpperInvariant());
    }
}
