using System.Windows;
using UI.ViewModels;

namespace UI.Views;

public partial class RecentSalesDialog : Window
{
    public RecentSalesDialog()
    {
        InitializeComponent();

              Loaded += (s, e) =>
        {
            if (DataContext is RecentSalesDialogViewModel vm)
            {
                vm.DialogClosed += () => Close();
            }
        };
    }
}
