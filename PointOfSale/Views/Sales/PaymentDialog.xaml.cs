using System.Windows;
using System.Windows.Input;
using UI.ViewModels;

namespace UI.Views;

public partial class PaymentDialog : Window
{
    public PaymentDialog()
    {
        InitializeComponent();

        // Subscribe to DialogClosed event from ViewModel
        Loaded += (s, e) =>
        {
            if (DataContext is PaymentDialogViewModel vm)
            {
                vm.DialogClosed += () => Close();
            }
        };

        // Handle keyboard shortcuts
        PreviewKeyDown += PaymentDialog_PreviewKeyDown;
    }

    private void PaymentDialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            // ESC key cancels the dialog
            Close();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            // Enter key confirms the payment
            if (DataContext is PaymentDialogViewModel vm && 
                vm.ConfirmPaymentCommand.CanExecute(null))
            {
                vm.ConfirmPaymentCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void CashReceived_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Allow only digits and decimal point
        e.Handled = !IsNumericInput(e.Text);
    }

    private void CashReceived_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        // Validate pasted content
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string pastedText = (string)e.DataObject.GetData(typeof(string));
            if (!IsNumericInput(pastedText))
                e.CancelCommand();
        }
        else
            e.CancelCommand();
    }

    private bool IsNumericInput(string text)
    {
        foreach (char c in text)
        {
            if (!char.IsDigit(c) && c != '.')
                return false;
        }
        return true;
    }
}
