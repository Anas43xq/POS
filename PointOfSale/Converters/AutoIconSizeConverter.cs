using System;
using System.Globalization;
using System.Windows.Data;

namespace UI.Controls
{
    /// <summary>
    /// values[0] = explicit IconSize (double?), values[1] = FontSize (double).
    /// If IconSize was set explicitly, use it. Otherwise derive it from FontSize
    /// so the currency symbol scales automatically with the text.
    /// </summary>
    public class AutoIconSizeConverter : IMultiValueConverter
    {
        // 0.8 keeps the glyph visually balanced against the digits at most sizes.
        public const double DefaultRatio = 0.8;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double? explicitSize = values[0] as double?;
            double fontSize = values[1] is double d ? d : 14.0;

            return explicitSize ?? (fontSize * DefaultRatio);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
