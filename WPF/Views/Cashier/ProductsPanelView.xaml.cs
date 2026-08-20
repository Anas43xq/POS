using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace UI.Views.Cashier;

public partial class ProductsPanelView : UserControl
{
    public ProductsPanelView()
    {
        InitializeComponent();
    }

    private void CategoryRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        ScrollProductsToTopAfterRefresh();
    }

    private void SubCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        ScrollProductsToTopAfterRefresh();
    }

    private void ScrollProductsToTopAfterRefresh()
    {
        Dispatcher.BeginInvoke(
            () =>
            {
                FindVisualChild<ScrollViewer>(ProductsItemsControl)?.ScrollToTop();
            },
            DispatcherPriority.Background);
    }

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindVisualChild<T>(child);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }
}
