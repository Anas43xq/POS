using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI.Views
{
    public partial class EndDayDialog : Window
    {
        private static readonly Regex DecimalRegex =
            new Regex(@"^\d{0,8}(\.\d{0,2})?$");

        public EndDayDialog()
        {
            InitializeComponent();
        }

        // Light validation only (no blocking typing issues)
        private void DecimalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;
            }
        }

        private void DecimalTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            string text = (string)e.DataObject.GetData(DataFormats.Text);

            if (!Regex.IsMatch(text, @"^[0-9]*(\.[0-9]*)?$"))
            {
                e.CancelCommand();
            }
        }

        // Final cleanup (important)
        private void DecimalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb) return;

            if (decimal.TryParse(tb.Text, out decimal value))
            {
                tb.Text = value.ToString("0.00");
            }
            else
            {
                tb.Text = "0.00";
            }
        }
    }
}