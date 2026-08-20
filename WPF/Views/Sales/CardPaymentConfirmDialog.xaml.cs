using System.Windows;
using UI.ViewModels;

namespace UI.Views;

public partial class CardPaymentConfirmDialog : Window
{
    public CardPaymentConfirmDialog()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (DataContext is CardPaymentConfirmDialogViewModel vm)
            {
                vm.RequestClose += confirmed =>
                {
                    DialogResult = confirmed;
                    Close();
                };
            }
        };
    }
}
