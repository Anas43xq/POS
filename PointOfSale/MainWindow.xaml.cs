using System.Windows;
using UI.ViewModels;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not MainViewModel mainViewModel)
                return;

            if (mainViewModel.CurrentViewModel is CashierDashboardViewModel cashier)
            {
                if (TryExecute(cashier.StartDayCommand, e, Key.F2) ||
                    TryExecute(cashier.EndDayCommand, e, Key.F3) ||
                    TryExecute(cashier.ShowRecentSalesCommand, e, Key.F4) ||
                    TryExecute(cashier.PayCashCommand, e, Key.F6) ||
                    TryExecute(cashier.PayCardCommand, e, Key.F7) ||
                    TryExecute(cashier.ClearSaleCommand, e, Key.F8) ||
                    TryExecute(cashier.LogoutCommand, e, Key.F9))
                {
                    return;
                }
            }

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
            if (sender is not TextBox textBox)
                return;

            string newText = GetPreviewText(textBox, e.Text);

            e.Handled = !IsValidMoneyInput(newText);
        }

        private void CashReceived_Pasting(object sender, DataObjectPastingEventArgs e)
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

            if (!IsValidMoneyInput(newText))
                e.CancelCommand();
        }

        private static string GetPreviewText(TextBox textBox, string input)
        {
            string currentText = textBox.Text;

            return currentText.Remove(textBox.SelectionStart, textBox.SelectionLength)
                              .Insert(textBox.SelectionStart, input);
        }

        private static bool IsValidMoneyInput(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            // Allows:
            // 123
            // 123.4
            // 123.45
            // Max 10 digits before decimal
            return Regex.IsMatch(text, @"^\d{0,7}(\.\d{0,2})?$");
        }

    }
}
