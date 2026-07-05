using BLL.Models;
using Contracts.Sales;
using Contracts.Transactions;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Windows;
using UI.Commands;
using UI.ViewModels;
using UI.Views;

namespace UI.ViewModels
{
    public partial class CashierDashboardViewModel
    {
        private void ShowStartDayDialog()
        {
            var viewModel = new StartDayDialogViewModel(_shiftService, _session);
            _dialogService.ShowDialog<StartDayDialog>(viewModel);
            LoadTopBar();
            RefreshCommandStates();
        }

        private void ShowEndDayDialog()
        {
            var viewModel = new EndDayDialogViewModel(_shiftService, _session);
            _dialogService.ShowDialog<EndDayDialog>(viewModel);

            LoadTopBar();
            RefreshCommandStates();

            if (_session.CurrentShift == null)
            {
                SaleItems.Clear();
                RecentSales.Clear();
            }
        }

        private void RefreshCommandStates()
        {
            ((RelayCommand)StartDayCommand).RaiseCanExecuteChanged();
            ((RelayCommand)EndDayCommand).RaiseCanExecuteChanged();
            PayCashCommand.RaiseCanExecuteChanged();
            PayCardCommand.RaiseCanExecuteChanged();
            if (AddProductCommand is AsyncRelayCommand addProductCmd)
            {
                addProductCmd.RaiseCanExecuteChanged();
            }
        }

        private async Task InitializeAsync()
        {
            LoadTopBar();
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            await LoadRecentSalesAsync();
            ProductsView.Refresh();
            UpdateNoProductsMessage();
        }

        private void LoadTopBar()
        {
            CashierName = _session.CurrentUser?.FullName ?? "Unknown Cashier";

            if (_session.CurrentShift != null && _session.CurrentShift.Status == DAL.Entities.ShiftStatus.Open)
            {
                ShiftStatus = $"Shift Open ({_session.CurrentShift.OpeningCash:C2})";
            }
            else if (_session.CurrentShift != null)
            {
                ShiftStatus = "Shift Closed";
            }
            else
            {
                ShiftStatus = "No Shift";
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                Result<List<Category>> result =
                    await _categoryService.GetAllCategoriesWithChildrenAsync();

                Categories.Clear();
                SubCategories.Clear();

                Categories.Add(new Category
                {
                    CategoryId = 0,
                    Name = "All"
                });

                if (result.IsSuccess && result.Value != null)
                {
                    foreach (Category category in result.Value.Where(c => !c.ParentCategoryId.HasValue))
                    {
                        Categories.Add(category);
                    }
                }

                SelectedCategory = Categories.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories for cashier dashboard");
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                Result<List<Product>> result =
                    await _productService.GetAllProductsWithTaxRateAsync();

                Products.Clear();

                if (result.IsSuccess && result.Value != null)
                {
                    foreach (Product product in result.Value)
                    {
                        Products.Add(product);
                    }
                }

                ProductsView.Refresh();
                UpdateNoProductsMessage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load products for cashier dashboard");
            }
        }

        public async Task LoadRecentSalesAsync()
        {
            RecentSales.Clear();

            if (_session.CurrentUser == null || _session.CurrentShift == null)
                return;

            try
            {
                List<RecentTransactionDto> result =
                    await _recentSale.GetRecentTransactionsByCashierId(
                        _session.CurrentUser.UserId,
                        _session.CurrentShift.ShiftId);

                if (result != null)
                {
                    foreach (RecentTransactionDto recentSale in result)
                    {
                        RecentSales.Add(recentSale);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load recent sales for cashier dashboard");
            }
        }

        private async Task SelectCategoryAsync(Category? category)
        {
            if (category == null)
                return;

            SelectedCategory = category;
            ProductsView.Refresh();
            UpdateNoProductsMessage();

            await Task.CompletedTask;
        }

        private async Task SelectSubCategoryAsync(Category? subCategory)
        {
            if (subCategory == null)
                return;

            SelectedCategory = subCategory;
            ProductsView.Refresh();
            UpdateNoProductsMessage();

            await Task.CompletedTask;
        }

        private void UpdateSubCategories()
        {
            SubCategories.Clear();

            if (SelectedCategory == null || SelectedCategory.CategoryId == 0)
                return;

            if (SelectedCategory.ChildCategories != null && SelectedCategory.ChildCategories.Any())
            {
                foreach (Category child in SelectedCategory.ChildCategories)
                    SubCategories.Add(child);
            }
            else if (SelectedCategory.ParentCategory != null)
            {
                foreach (Category sibling in SelectedCategory.ParentCategory.ChildCategories)
                {
                    if (sibling.CategoryId != SelectedCategory.CategoryId)
                        SubCategories.Add(sibling);
                }
            }
        }

        private bool FilterProduct(object item)
        {
            if (item is not Product product)
                return false;

            if (!IsProductInSelectedCategory(product))
                return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string search = SearchText.Trim();
                if (!product.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsProductInSelectedCategory(Product product)
        {
            if (SelectedCategory == null || SelectedCategory.CategoryId == 0)
                return true;

            if (product.CategoryId == SelectedCategory.CategoryId)
                return true;

            if (SelectedCategory.ChildCategories != null && SelectedCategory.ChildCategories.Any())
            {
                return SelectedCategory.ChildCategories.Any(child => child.CategoryId == product.CategoryId);
            }

            return SelectedCategory.ParentCategory != null &&
                   SelectedCategory.ParentCategory.ChildCategories.Any(child => child.CategoryId == product.CategoryId);
        }

        private void UpdateNoProductsMessage()
        {
            ShowNoProductsMessage = ProductsView.IsEmpty && !string.IsNullOrWhiteSpace(SearchText);
        }

        private void SaleItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CartItem item in e.NewItems)
                {
                    item.PropertyChanged += CartItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CartItem item in e.OldItems)
                    item.PropertyChanged -= CartItem_PropertyChanged;
            }

            RefreshTotals();
            OnPropertyChanged(nameof(SaleItemsCount));
            ClearSaleCommand.RaiseCanExecuteChanged();
            PayCashCommand.RaiseCanExecuteChanged();
            PayCardCommand.RaiseCanExecuteChanged();
            IncreaseSelectedQuantityCommand.RaiseCanExecuteChanged();
            DecreaseSelectedQuantityCommand.RaiseCanExecuteChanged();
            RemoveSelectedSaleItemCommand.RaiseCanExecuteChanged();
        }

        private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Quantity))
            {
                RefreshTotals();
            }
        }
    }
}
