using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace UI.Views
{
    public partial class StartDayDialog : Window
    {
        // Accept digits and at most one decimal point. Pasted values
        // are normalised in PreviewTextInput / Pasting handlers below.
        private static readonly Regex NumericInputRegex =
            new Regex(@"^\d{0,7}(\.\d{0,2})?$", RegexOptions.Compiled);

        public StartDayDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Focus the opening-cash textbox when the dialog loads and
        /// pre-select its contents so the cashier can just type a
        /// replacement value. Uses <see cref="DispatcherPriority.Input"/>
        /// so the textbox is fully realized before we focus it.
        /// </summary>
        private void OpeningCashBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Dispatcher.BeginInvoke(
                    DispatcherPriority.Input,
                    new Action(() =>
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                    }));
            }
        }

        /// <summary>
        /// Enter inside the <c>OpeningCash</c> textbox is normally
        /// swallowed by the textbox itself, so the <c>IsDefault</c>
        /// button never fires while the user is typing. We intercept
        /// Enter here and explicitly invoke <c>StartDayCommand</c>
        /// so the cashier can submit without having to Tab/click the
        /// Open-Shift button.
        /// </summary>
        private void OpeningCashBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            e.Handled = true;

            if (DataContext is ViewModels.StartDayDialogViewModel vm &&
                vm.StartDayCommand.CanExecute(null))
            {
                vm.StartDayCommand.Execute(null);
            }
        }

        /// <summary>
        /// Reject any character that would make the resulting text
        /// non-numeric (e.g. letters, symbols). Allows digits and
        /// at most one decimal separator.
        /// </summary>
        private void OpeningCashBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            string newText = GetPreviewText(textBox, e.Text);

            // Accept only what would still parse as a valid amount.
            e.Handled = !IsValidNumericInput(newText);
        }

        private void OpeningCashBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            string pastedText = (string)e.DataObject.GetData(typeof(string))!;
            string newText = GetPreviewText(textBox, pastedText);

            if (!IsValidNumericInput(newText))
                e.CancelCommand();
        }

        private static string GetPreviewText(TextBox textBox, string input)
        {
            string currentText = textBox.Text;

            return currentText.Remove(textBox.SelectionStart, textBox.SelectionLength)
                              .Insert(textBox.SelectionStart, input);
        }

        private static bool IsValidNumericInput(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            // Max 7 digits before decimal, 2 after.
            return NumericInputRegex.IsMatch(text);
        }
    }
}
