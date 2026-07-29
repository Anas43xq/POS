using System.Windows.Controls;

namespace UI.Views
{
    /// <summary>
    /// Reports filter bar. Inherits the host page's DataContext (ReportViewModel);
    /// expects IsSalesMode/IsProductMode, ReportCommand, FilterTodayCommand/
    /// FilterThisWeekCommand/FilterMonthCommand/ShowPeriodFilterCommand/
    /// ApplyPeriodFilterCommand, IsPeriodFilterVisible, FromDate/ToDate,
    /// ProductGridVisibility, Products, and SelectedProduct.
    /// </summary>
    public partial class ReportFilterPanelControl : UserControl
    {
        public ReportFilterPanelControl()
        {
            InitializeComponent();
        }
    }
}
