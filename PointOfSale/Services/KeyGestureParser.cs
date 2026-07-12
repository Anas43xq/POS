using System.Windows.Input;

namespace UI.Services;

public static class KeyGestureParser
{
    public static bool TryParse(string gesture, out Key key, out ModifierKeys modifiers)
    {
        key = Key.None;
        modifiers = ModifierKeys.None;

        if (string.IsNullOrWhiteSpace(gesture))
            return false;

        var parts = gesture.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return false;

        modifiers = ModifierKeys.None;
        key = Key.None;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (string.Equals(part, "Ctrl", StringComparison.OrdinalIgnoreCase)
                || string.Equals(part, "Control", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= ModifierKeys.Control;
            }
            else if (string.Equals(part, "Alt", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= ModifierKeys.Alt;
            }
            else if (string.Equals(part, "Shift", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= ModifierKeys.Shift;
            }
            else if (string.Equals(part, "Win", StringComparison.OrdinalIgnoreCase)
                || string.Equals(part, "Meta", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= ModifierKeys.Windows;
            }
            else
            {
                if (!Enum.TryParse<Key>(part, ignoreCase: true, out var parsedKey))
                    return false;

                key = parsedKey;
            }
        }

        return key != Key.None;
    }

    public static string ToDisplayString(Key key, ModifierKeys modifiers)
    {
        var parts = new List<string>();

        if (modifiers.HasFlag(ModifierKeys.Control))
            parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Alt))
            parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Shift))
            parts.Add("Shift");
        if (modifiers.HasFlag(ModifierKeys.Windows))
            parts.Add("Win");

        parts.Add(key.ToString());

        return string.Join("+", parts);
    }
}
