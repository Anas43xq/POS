using System.Windows.Controls;

namespace UI.Views
{
    /// <summary>
    /// Full-width Key Metrics strip. Inherits the host page's DataContext;
    /// expects TotalSales, AverageSales, TotalCash, TotalCard, TotalOrders.
    /// </summary>
    public partial class KpiMetricsControl : UserControl
    {
        public KpiMetricsControl()
        {
            InitializeComponent();
        }
    }
}
