using System.Windows;
using UI.ViewModels;

namespace UI.Views;

public partial class RecentSalesDialog : Window
{
    public RecentSalesDialog()
    {
        InitializeComponent();

        // Subscribe to DialogClosed event from ViewModel
        Loaded += (s, e) =>
        {
            if (DataContext is RecentSalesDialogViewModel vm)
            {
                vm.DialogClosed += () => Close();
            }
        };
    }
}
