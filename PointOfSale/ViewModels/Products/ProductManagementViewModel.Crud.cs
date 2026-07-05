using System;
using System.Threading.Tasks;
using System.Windows;
using UI.Views;

namespace UI.ViewModels
{
    public partial class ProductManagementViewModel
    {
        private void AddProduct()
        {
            var formVm = new ProductFormViewModel(
                _productService,
                _categoryService,
                _taxRateService,
                this);

            _dialogService.ShowDialog<ProductFormView>(formVm);
        }

        private void EditProduct()
        {
            if (SelectedProduct == null)
                return;

            var formVm = new ProductFormViewModel(
                _productService,
                _categoryService,
                _taxRateService,
                this,
                SelectedProduct);

            _dialogService.ShowDialog<ProductFormView>(formVm);
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete \"{SelectedProduct.Name}\"? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _productService.DeleteProductAsync(SelectedProduct.Id);
                await RefreshDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to delete product: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public async Task RefreshDataAsync()
        {
            CategorySearchText = string.Empty;
            ProductSearchText = string.Empty;
            SelectedCategory = null;
            SelectedProduct = null;
            await LoadDataAsync();
        }

        private bool CanEditProduct() => SelectedProduct != null;

        private bool CanDeleteProduct() => SelectedProduct != null;
    }
}
