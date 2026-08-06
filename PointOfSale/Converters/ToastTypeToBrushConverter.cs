using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using UI.Services;

namespace UI.Converters
{
    /// <summary>
    /// Maps a <see cref="ToastType"/> to one of the brush resources defined
    /// in Resources/Common/Brushes.xaml. The converter parameter selects
    /// which tone within that type's palette to use:
    /// "Accent" (default), "Dark", "Light", or "Border".
    /// </summary>
    public class ToastTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ToastType type)
                return Brushes.Transparent;

            string tone = parameter as string ?? "Accent";

            string key = type switch
            {
                ToastType.Success => tone switch
                {
                    "Dark" => "SuccessDark",
                    "Light" => "SuccessLight",
                    "Border" => "SuccessBorder",
                    _ => "SuccessGreen"
                },
                ToastType.Error => tone switch
                {
                    "Dark" => "DangerDark",
                    "Light" => "DangerLight",
                    "Border" => "DangerBorder",
                    _ => "DangerRed"
                },
                ToastType.Warning => tone switch
                {
                    "Dark" => "WarningDark",
                    "Light" => "WarningLight",
                    "Border" => "WarningBorder",
                    _ => "WarningAmber"
                },
                _ => tone switch
                {
                    "Dark" => "InfoDark",
                    "Light" => "InfoLight",
                    "Border" => "InfoBorder",
                    _ => "InfoBlue"
                }
            };

            return Application.Current?.TryFindResource(key) ?? Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
