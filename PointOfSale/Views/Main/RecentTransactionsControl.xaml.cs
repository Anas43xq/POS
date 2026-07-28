using System.Windows.Controls;

namespace UI.Views
{
    /// <summary>
    /// Recent Transactions card. Inherits the host page's DataContext; expects
    /// RecentTransactions (ReceiptNumber, GrandTotal, PaymentMethod, TransactionDate),
    /// OpenReceiptCommand, and ShowAllTransactionsCommand.
    /// </summary>
    public partial class RecentTransactionsControl : UserControl
    {
        public RecentTransactionsControl()
        {
            InitializeComponent();
        }
    }
}
