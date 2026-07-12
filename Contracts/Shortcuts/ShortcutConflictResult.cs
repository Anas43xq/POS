using Contracts.Enum;

namespace Contracts.Shortcuts;

public sealed class ShortcutConflictResult
{
    public bool HasConflicts => Conflicts.Count > 0;

    public List<ShortcutConflict> Conflicts { get; set; } = [];
}

public sealed class ShortcutConflict
{
    public required ShortcutAction ExistingAction { get; init; }

    public required string ExistingGesture { get; init; }

    public required ShortcutAction ConflictingAction { get; init; }

    public required string ConflictingGesture { get; init; }
}
