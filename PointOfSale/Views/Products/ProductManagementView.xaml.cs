using System.Windows;
using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Views
{
    public partial class ProductManagementView : UserControl
    {
        public ProductManagementView()
        {
            InitializeComponent();
        }

        private void CategoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ProductManagementViewModel vm)
            {
                vm.SelectedCategory = e.NewValue as CategoryNodeViewModel;
            }
        }

        private void ProductsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is ProductManagementViewModel vm)
            {
                vm.EditProductCommand.Execute(null);
            }
        }
    }
}
