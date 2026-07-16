using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UI.Converters;

/// <summary>
/// Converts a boolean (IsSelected on a modifier chip) to a foreground brush.
/// True  → White (chip is highlighted with accent background)
/// False → TextDark (default brand text color)
/// </summary>
public sealed class BoolToChipForegroundConverter : IValueConverter
{
    private static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
    private static readonly SolidColorBrush TextDarkBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x5C));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
            return WhiteBrush;
        return TextDarkBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}