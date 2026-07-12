using System.Globalization;
using System.Windows.Data;
using Contracts.Enum;
using UI.Services;

namespace UI.Converters;

public class ShortcutGestureConverter : IValueConverter
{
    private static IKeyboardShortcutService? _shortcutService;

    public static void Initialize(IKeyboardShortcutService shortcutService)
    {
        _shortcutService = shortcutService;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ShortcutAction action || _shortcutService == null)
            return string.Empty;

        var bindings = _shortcutService.GetActiveBindings();
        var binding = bindings.FirstOrDefault(b => b.Action == action);
        return binding?.KeyGesture ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
