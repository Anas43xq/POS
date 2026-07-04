using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.ViewModels;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for CashierDashboardView.xaml
    /// </summary>
    public partial class CashierDashboardView : UserControl
    {
        public CashierDashboardView()
        {
            InitializeComponent();
            PreviewKeyDown += CashierDashboardView_PreviewKeyDown;
        }

        private void CashierDashboardView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not CashierDashboardViewModel viewModel)
                return;

            if (Keyboard.FocusedElement is TextBox)
                return;

            if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                if (viewModel.IncreaseSelectedQuantityCommand.CanExecute(null))
                    viewModel.IncreaseSelectedQuantityCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Subtract || e.Key == Key.OemMinus || e.Key == Key.Back)
            {
                if (viewModel.DecreaseSelectedQuantityCommand.CanExecute(null))
                    viewModel.DecreaseSelectedQuantityCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Delete)
            {
                if (viewModel.RemoveSelectedSaleItemCommand.CanExecute(null))
                    viewModel.RemoveSelectedSaleItemCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
