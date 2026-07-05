using Contracts.Sales;
using Contracts.Transactions;
using Contracts.Shifts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels
{
    public partial class HomeViewModel
    {
        private void OnFilterToday()
        {
            CurrentFilterMode = DashboardFilterMode.Today;
            IsPeriodFilterVisible = false;
            _ = LoadKpisAsync();
        }

        private void OnFilterThisWeek()
        {
            CurrentFilterMode = DashboardFilterMode.Week;
            IsPeriodFilterVisible = false;
            _ = LoadKpisAsync();
        }

        private void OnFilterThisMonth()
        {
            CurrentFilterMode = DashboardFilterMode.Month;
            IsPeriodFilterVisible = false;
            _ = LoadKpisAsync();
        }

        private void OnShowPeriodFilter()
        {
            CurrentFilterMode = DashboardFilterMode.Period;
            IsPeriodFilterVisible = true;
        }

        private void OnApplyPeriodFilter()
        {
            if (!PeriodFromDate.HasValue)
            {
                ErrorMessage = "From date is required for a custom period.";
                return;
            }

            ErrorMessage = string.Empty;
            CurrentFilterMode = DashboardFilterMode.Period;
            IsPeriodFilterVisible = true;
            _ = LoadKpisAsync();
        }

        private async Task LoadAsyncSafe()
        {
            try
            {
                await LoadAsyncCore();
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load dashboard data.");
                ErrorMessage = "Unable to load dashboard data.";
            }
        }

        private async Task LoadAsyncCore()
        {
            await LoadKpisAsync();
            await LoadTopProductsAsync();
            await LoadRecentTransactionsAsync();
            await LoadShiftSummaryAsync();
        }

        private async Task LoadRecentTransactionsAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                RecentTransactions.Clear();
                var transactions = await _recentTransactionService.GetRecentTransactionsAsync(10);
                foreach (var transaction in transactions)
                {
                    RecentTransactions.Add(transaction);
                }

                OnPropertyChanged(nameof(HasRecentTransactions));
                OnPropertyChanged(nameof(HasNoRecentTransactions));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load recent transactions.");
                ErrorMessage = "Unable to load recent transactions.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadTopProductsAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                TopProducts.Clear();

                var products = await GetFilteredTopProductsAsync(7);
                foreach (var product in products)
                {
                    TopProducts.Add(product);
                }

                OnPropertyChanged(nameof(HasTopProducts));
                OnPropertyChanged(nameof(HasNoTopProducts));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load top products.");
                ErrorMessage = "Unable to load top products.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadShiftSummaryAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ShiftSummaries.Clear();
                var shiftSummaries = await _shiftSummaryService.GetLatestShiftSummariesAsync(5);
                foreach (var summary in shiftSummaries)
                {
                    ShiftSummaries.Add(summary);
                }

                OnPropertyChanged(nameof(HasShiftSummaries));
                OnPropertyChanged(nameof(HasNoShiftSummaries));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load shift summary.");
                ErrorMessage = "Unable to load shift summary.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OpenShiftManagement(object? obj)
        {
            ViewAllShiftsRequested?.Invoke();
        }

        private void ShowAllTransactions(object? obj)
        {
            ShowAllTransactionsRequested?.Invoke();
        }

        private void OpenReceipt(RecentTransactionDto? transaction)
        {
            if (transaction == null || transaction.TransactionId <= 0)
                return;

            _receiptDisplayService.ShowReceipt(transaction.TransactionId);
        }

        public async Task LoadKpisAsync(CancellationToken ct = default)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var request = new GetTransactionKpisRequest
                {
                    PeriodType = CurrentFilterMode switch
                    {
                        DashboardFilterMode.Today => "Today",
                        DashboardFilterMode.Week => "Week",
                        DashboardFilterMode.Month => "Month",
                        DashboardFilterMode.Period => "Custom",
                        _ => "Today"
                    },
                    FromDate = PeriodFromDate,
                    ToDate = PeriodToDate
                };

                if (request.PeriodType != "Custom")
                {
                    request.FromDate = null;
                    request.ToDate = null;
                }

                var dto = await _kpiService.GetKpisAsync(request, ct);
                if (dto != null)
                {
                    TotalSales = dto.TotalSales;
                    AverageSales = dto.AverageSale;
                    TotalCash = dto.TotalCash;
                    TotalCard = dto.TotalCard;
                    TotalOrders = dto.TotalOrders;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<List<TopProductDto>> GetFilteredTopProductsAsync(int take)
        {
            var products = await _topProductService.GetTopProductsAsync(take);
            return products;
        }
    }
}
