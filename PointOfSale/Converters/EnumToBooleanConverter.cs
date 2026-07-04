using System;
using System.Globalization;
using System.Windows.Data;

namespace UI.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string enumValue = value.ToString()!;
            string targetValue = parameter.ToString()!;
            return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Binding.DoNothing;

            if (value is bool useValue && useValue)
            {
                return Enum.Parse(targetType, parameter.ToString()!);
            }

            return Binding.DoNothing;
        }
    }
}
