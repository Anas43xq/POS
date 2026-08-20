using BLL.DTOs;
using BLL.Models;
using Contracts.Sales;
using Contracts.Transactions;
using Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Windows;
using UI.Commands;
using UI.Services;
using UI.ViewModels;
using UI.Views;

namespace UI.ViewModels
{
    public partial class CashierDashboardViewModel
    {
        private void ShowStartDayDialog()
        {
            var viewModel = _viewModelFactory.Create<StartDayDialogViewModel>();
            _dialogService.ShowDialog<StartDayDialog>(viewModel);
            LoadTopBar();
            RefreshCommandStates();
        }

        private void ShowEndDayDialog()
        {
            var viewModel = _viewModelFactory.Create<EndDayDialogViewModel>();
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
            var stopwatch = Stopwatch.StartNew();
            LoadTopBar();
            await Task.WhenAll(
                LoadCategoriesAsync(),
                LoadProductsAsync());
            await LoadRecentSalesAsync();
            RefreshProductView();
            TxpTrace.WriteLine(
                $"[TXP] - Cashier dashboard initial load completed in {stopwatch.ElapsedMilliseconds} ms");
        }

        private void LoadTopBar()
        {
            CashierName = _session.CurrentUser?.FullName ?? "Unknown Cashier";

            if (_session.CurrentShift != null && _session.CurrentShift.Status == Contracts.Enum.ShiftStatus.Open)
            {
                ShiftStatus = _localization.GetString(
                    "Cashier.ShiftOpen",
                    _session.CurrentShift.OpeningCash.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-AE")));
            }
            else if (_session.CurrentShift != null)
            {
                ShiftStatus = _localization.GetString("Cashier.ShiftClosed");
            }
            else
            {
                ShiftStatus = _localization.GetString("Cashier.NoShift");
            }
        }

        #region Database Calls
        private async Task LoadCategoriesAsync()
        {
            try
            {
                var languageCode = _localization.CurrentLanguage.FilePrefix;

                Result<List<CategoryDto>> result =
                    await _categoryService.GetAllCategoriesWithChildrenAsync(languageCode);

                Categories.Clear();
                SubCategories.Clear();

                Categories.Add(new CategoryDto
                {
                    CategoryId = 0,
                    Name = _localization.GetString("Cashier.AllCategories")
                });

                if (result.IsSuccess && result.Value != null)
                {
                    foreach (CategoryDto category in result.Value.Where(c => !c.ParentCategoryId.HasValue))
                    {
                        Categories.Add(category);
                    }

                }

                SelectedCategory = Categories.FirstOrDefault();
                if (SelectedCategory != null)
                    SelectedCategory.IsSelected = true;

                RefreshProductView();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories for cashier dashboard");
                _notifications.ShowError("Failed to load categories. Please try again.");
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var languageCode = _localization.CurrentLanguage.FilePrefix;

                Result<List<ProductDto>> result =
                    await _productService.GetAllVariantsAsync(languageCode);

                Products.Clear();

                if (result.IsSuccess && result.Value != null)
                {
                    foreach (ProductDto product in result.Value)
                    {
                        Products.Add(product);
                    }
                }

                RefreshProductView();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load products for cashier dashboard");
                _notifications.ShowError("Failed to load products. Please try again.");
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
                _notifications.ShowError("Failed to load recent sales.");
            }
        }

        #endregion


        public async Task OpenSetting()
        {
            var vm = _viewModelFactory.Create<SettingsViewModel>();
            _dialogService.ShowDialog<SettingsWindow>(vm);
            await Task.CompletedTask;
        }

        private async Task SelectParentCategoryAsync(CategoryDto? category)
        {
            await SelectCategoryAsync(category);
        }

        private async Task SelectSubCategoryAsync(CategoryDto? subCategory)
        {
            await SelectCategoryAsync(subCategory);
        }

        private async Task SelectCategoryAsync(CategoryDto? category)
        {
            if (category == null)
                return;

            // Toggle IsSelected on old/new categories so the
            // DataTrigger in ProductsPanelView can highlight the
            // active sidebar / sub-category button.
            if (SelectedCategory != null)
                SelectedCategory.IsSelected = false;

            category.IsSelected = true;
            SelectedCategory = category;
            RefreshProductView();

            await Task.CompletedTask;
        }


        private bool FilterProduct(object item)
        {
            if (item is not ProductDto product)
                return false;

            var search = SearchText?.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var match = product.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || product.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || product.EnglishDisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || product.EnglishName.Contains(search, StringComparison.OrdinalIgnoreCase);

                if (!match)
                    return false;
            }

            if (SelectedCategory == null || SelectedCategory.CategoryId == 0)
                return true;

            if (product.CategoryId == SelectedCategory.CategoryId)
                return true;

            // If the selected category has children (i.e. it's a parent),
            // include products from any of those children.
            if (SelectedCategory.ChildCategories != null && SelectedCategory.ChildCategories.Any())
            {
                return SelectedCategory.ChildCategories.Any(child => child.CategoryId == product.CategoryId);
            }

            // Selected category is a leaf (sub-category) — only its own products are shown.
            return false;
        }

        private void UpdateNoProductsMessage()
        {
            ShowNoProductsMessage = ProductsView.IsEmpty;
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
            OnPropertyChanged(nameof(CartCountDisplay));
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
