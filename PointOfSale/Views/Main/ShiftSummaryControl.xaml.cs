using System.Windows.Controls;

namespace UI.Views
{
    /// <summary>
    /// Shift Summary card. Inherits the host page's DataContext; expects
    /// ShiftSummaries (CashierName, CashDifference, IsShortfall, OpenedAt,
    /// ClosedAtDisplay) and OpenShiftManagementCommand.
    /// </summary>
    public partial class ShiftSummaryControl : UserControl
    {
        public ShiftSummaryControl()
        {
            InitializeComponent();
        }
    }
}
