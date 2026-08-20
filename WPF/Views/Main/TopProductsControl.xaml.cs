using System.Windows.Controls;

namespace UI.Views
{
    /// <summary>
    /// Top Products card. Inherits the host page's DataContext; expects a
    /// TopProducts collection with ProductName and TotalSales.
    /// </summary>
    public partial class TopProductsControl : UserControl
    {
        public TopProductsControl()
        {
            InitializeComponent();
        }
    }
}
