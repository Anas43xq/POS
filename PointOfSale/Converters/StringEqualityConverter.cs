using System;
using System.Globalization;
using System.Windows.Data;

namespace UI.Converters;

/// <summary>
/// Returns <c>true</c> when the bound value equals the string
/// <see cref="ConverterParameter"/>.  Used to drive
/// <c>RadioButton.IsChecked</c> from a string-based ViewModel
/// property (e.g. "Day" / "Week" / "Month" / "Period").
/// </summary>
public sealed class StringEqualityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && parameter is string param)
            return string.Equals(str, param, StringComparison.Ordinal);

        return false;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        // When a RadioButton is checked, push its parameter back to the source.
        if (value is true && parameter is string param)
            return param;

        return Binding.DoNothing;
    }
}