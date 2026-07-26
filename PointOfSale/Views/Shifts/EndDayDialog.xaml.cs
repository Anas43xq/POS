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

        /// <summary>
        /// Focus the closing-cash textbox when the dialog loads and
        /// pre-select its contents so the manager can just type a
        /// replacement value.
        /// </summary>
        private void ClosingCashBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                    }));
            }
        }

        /// <summary>
        /// Enter inside the <c>ClosingCash</c> textbox is normally
        /// swallowed by the textbox itself, so the <c>EndDayCommand</c>
        /// never fires while the user is typing. We intercept Enter
        /// here and explicitly invoke <c>EndDayCommand</c> so the
        /// manager can submit without having to Tab/click the
        /// End-Day button.
        /// </summary>
        private void ClosingCashBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            e.Handled = true;

            if (DataContext is ViewModels.EndDayDialogViewModel vm &&
                vm.EndDayCommand.CanExecute(null))
            {
                vm.EndDayCommand.Execute(null);
            }
        }

        /// <summary>
        /// The End-Day button uses a custom <c>ControlTemplate</c> that
        /// replaces the default ButtonChrome, so the built-in
        /// click-to-Command wiring does not fire on its own. We
        /// explicitly invoke the command here so a mouse click
        /// on the button still submits the dialog.
        /// </summary>
        private void EndDayButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.EndDayDialogViewModel vm &&
                vm.EndDayCommand.CanExecute(null))
            {
                vm.EndDayCommand.Execute(null);
            }
        }
    }
}
