using POS.Contracts.Receipts;
using System;
using System.Globalization;
using System.Windows.Data;

namespace UI.Converters;

/// <summary>
/// Converts a <see cref="ReceiptModifierDto"/> to a display string
/// for the receipt. Shows "+ OptionName" for quantity 1,
/// "+ OptionName ×N" for quantity > 1.
/// </summary>
[ValueConversion(typeof(ReceiptModifierDto), typeof(string))]
public class ModifierLineConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ReceiptModifierDto modifier)
            return string.Empty;

        if (modifier.Quantity > 1)
            return $"+ {modifier.OptionName} ×{modifier.Quantity}";

        return $"+ {modifier.OptionName}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}