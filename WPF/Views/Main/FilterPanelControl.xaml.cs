using System.Windows.Controls;

namespace UI.Views
{
    /// <summary>
    /// Reusable date-range filter bar used at the top of Manager dashboard pages.
    /// Deliberately has no DataContext of its own — it inherits the host page's
    /// ViewModel, so it works with any VM that exposes the expected
    /// commands/properties (FilterTodayCommand, PeriodFromDate, RefreshCommand, etc.).
    /// </summary>
    public partial class FilterPanelControl : UserControl
    {
        public FilterPanelControl()
        {
            InitializeComponent();
        }
    }
}
