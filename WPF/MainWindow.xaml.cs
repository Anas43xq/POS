using System.Windows;
using System.Windows.Input;
using Contracts.Enum;
using UI.Services;
using UI.ViewModels;

namespace UI
{
    public partial class MainWindow : Window
    {

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }


        private static bool TryExecute(ICommand command, KeyEventArgs e, Key key)
        {
            if (e.Key != key)
                return false;

            if (command.CanExecute(null))
                command.Execute(null);

            e.Handled = true;
            return true;
        }

        private void CashReceived_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox textBox)
                return;

            string newText = GetPreviewText(textBox, e.Text);
            e.Handled = !IsValidMoneyInput(newText);
        }

        private void CashReceived_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox textBox)
                return;

            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            string pastedText = (string)e.DataObject.GetData(typeof(string))!;
            string newText = GetPreviewText(textBox, pastedText);

            if (!IsValidMoneyInput(newText))
                e.CancelCommand();
        }

        private static string GetPreviewText(System.Windows.Controls.TextBox textBox, string input)
        {
            string currentText = textBox.Text;
            return currentText.Remove(textBox.SelectionStart, textBox.SelectionLength)
                              .Insert(textBox.SelectionStart, input);
        }

        private static bool IsValidMoneyInput(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            return System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d{0,7}(\.\d{0,2})?$");
        }
    }
}
