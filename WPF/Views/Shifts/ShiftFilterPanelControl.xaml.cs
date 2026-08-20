using System.Windows.Controls;

namespace UI.Views;

/// <summary>
/// Shift Management filter bar. Inherits the host page's DataContext; expects
/// ShiftStatusFilterOptions / SelectedShiftStatusFilter for the status filter,
/// the four LoadXxxCommand + ApplyPeriodCommand + IsPeriodFilterVisible +
/// FromDate / ToDate for the date-range (forwarded to the shared
/// <see cref="UI.Controls.DateRangeFilterControl"/>), and RefreshCommand.
/// </summary>
public partial class ShiftFilterPanelControl : UserControl
{
    public ShiftFilterPanelControl()
    {
        InitializeComponent();
    }
}
