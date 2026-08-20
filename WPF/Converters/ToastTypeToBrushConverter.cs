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
    /// in Themes/Tokens/Colors.xaml. The converter parameter selects
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
                    "Dark" => "Color.Success.Dark",
                    "Light" => "Color.Success.Light",
                    "Border" => "Color.Success.Border",
                    _ => "Color.Success.Default"
                },
                ToastType.Error => tone switch
                {
                    "Dark" => "Color.Danger.Dark",
                    "Light" => "Color.Danger.Light",
                    "Border" => "Color.Danger.Border",
                    _ => "Color.Danger.Default"
                },
                ToastType.Warning => tone switch
                {
                    "Dark" => "Color.Warning.Dark",
                    "Light" => "Color.Warning.Light",
                    "Border" => "Color.Warning.Border",
                    _ => "Color.Warning.Default"
                },
                _ => tone switch
                {
                    "Dark" => "Color.Info.Dark",
                    "Light" => "Color.Info.Light",
                    "Border" => "Color.Info.Border",
                    _ => "Color.Info.Default"
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
