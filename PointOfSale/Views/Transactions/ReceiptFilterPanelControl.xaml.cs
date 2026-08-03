using System.Windows.Controls;

namespace UI.Views
{
    /// <summary>
    /// Receipt Management filter bar. Inherits the host page's
    /// <c>ReceiptManagementViewModel</c> DataContext; expects
    /// <c>SearchText</c>, the four <c>LoadXxxCommand</c> +
    /// <c>ApplyPeriodCommand</c> + <c>IsPeriodFilterVisible</c> +
    /// <c>DateFrom</c> / <c>DateTo</c> for the shared
    /// <see cref="UI.Controls.DateRangeFilterControl"/>, <c>Suppliers</c>
    /// + <c>SelectedSupplier</c> for the supplier combo,
    /// <c>CategoryFilter</c> for the category textbox, and
    /// <c>ApplyFiltersCommand</c>, <c>ResetFiltersCommand</c>,
    /// <c>RefreshCommand</c> for the action buttons.
    /// </summary>
    public partial class ReceiptFilterPanelControl : UserControl
    {
        public ReceiptFilterPanelControl()
        {
            InitializeComponent();
        }
    }
}
