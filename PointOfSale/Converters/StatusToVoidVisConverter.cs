using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DAL.Entities;

namespace UI.Converters
{
    /// <summary>
    /// Maps a transaction row's status to a <see cref="Visibility"/> for the
    /// "Void" action button.
    ///
    /// The binding source is <c>TransactionListItemDto.Status</c>, which is
    /// produced by the SQL CASE expression in <c>SP_GetTransactionsList</c>
    /// as the string name of <see cref="TransactionStatus"/>
    /// (<c>"Pending"</c> / <c>"Completed"</c> / <c>"Voided"</c>).  We parse
    /// it to the canonical enum so the check is type-safe.
    ///
    /// Visible only when the status is <see cref="TransactionStatus.Completed"/>;
    /// a Pending or already-Voided transaction cannot be voided.
    /// </summary>
    public class StatusToVoidVisConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is string text &&
                Enum.TryParse<TransactionStatus>(text, ignoreCase: true, out var status) &&
                status == TransactionStatus.Completed)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
