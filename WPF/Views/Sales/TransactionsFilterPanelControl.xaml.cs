using System.Windows.Controls;

namespace UI.Views
{
    /// <summary>
    /// Transactions filter bar. Inherits the host page's DataContext; expects
    /// LoadDayCommand/LoadWeekCommand/LoadMonthCommand/LoadPeriodCommand,
    /// IsPeriodFilterVisible, FromDate/ToDate, ApplyPeriodCommand,
    /// StatusOptions/SelectedStatusFilter, VoidTransactionCommand,
    /// SelectedTransaction, and RefreshCommand.
    /// </summary>
    public partial class TransactionsFilterPanelControl : UserControl
    {
        public TransactionsFilterPanelControl()
        {
            InitializeComponent();
        }
    }
}
