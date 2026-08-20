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
        private static readonly Regex NumericInputRegex =
            new Regex(@"^\d{0,7}(\.\d{0,2})?$", RegexOptions.Compiled);

        public StartDayDialog()
        {
            InitializeComponent();
        }

        /// <summary>Focuses the opening-cash textbox on load and selects its contents.</summary>
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

        /// <summary>Intercepts Enter in the opening-cash field so it can trigger StartDayCommand.</summary>
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

        /// <summary>Rejects input that would make the amount invalid.</summary>
        private void OpeningCashBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            string newText = GetPreviewText(textBox, e.Text);
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

            return NumericInputRegex.IsMatch(text);
        }
    }
}
