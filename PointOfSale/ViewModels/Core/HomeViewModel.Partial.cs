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
        private GetTransactionKpisRequest BuildFilterRequest()
        {
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

            return request;
        }

        private void OnFilterToday()
        {
            CurrentFilterMode = DashboardFilterMode.Today;
            IsPeriodFilterVisible = false;
            _ = LoadFilteredSectionsAsync();
        }

        private void OnFilterThisWeek()
        {
            CurrentFilterMode = DashboardFilterMode.Week;
            IsPeriodFilterVisible = false;
            _ = LoadFilteredSectionsAsync();
        }

        private void OnFilterThisMonth()
        {
            CurrentFilterMode = DashboardFilterMode.Month;
            IsPeriodFilterVisible = false;
            _ = LoadFilteredSectionsAsync();
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
            _ = LoadFilteredSectionsAsync();
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
            _isInitialLoadBusy = true;
            try
            {
                await LoadRecentTransactionsAsync();
                await LoadShiftSummaryAsync();

                var request = BuildFilterRequest();
                await Task.WhenAll(
                    LoadKpisAsync(request),
                    LoadTopProductsAsync(request));
            }
            finally
            {
                _isInitialLoadBusy = false;
            }
        }

        private async Task LoadFilteredSectionsAsync()
        {
            if (_isInitialLoadBusy)
                return;

            try
            {
                ErrorMessage = string.Empty;
                var request = BuildFilterRequest();
                await Task.WhenAll(
                    LoadKpisAsync(request),
                    LoadTopProductsAsync(request));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load filtered dashboard data.");
                ErrorMessage = "Unable to load filtered data.";
            }
        }

        private async Task LoadRecentTransactionsAsync()
        {
            try
            {
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
        }

        private async Task LoadTopProductsAsync(GetTransactionKpisRequest request)
        {
            try
            {
                TopProducts.Clear();
                var products = await _topProductService.GetTopProductsAsync(request, 7);
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
        }

        private async Task LoadShiftSummaryAsync()
        {
            try
            {
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

        public async Task LoadKpisAsync(GetTransactionKpisRequest request, CancellationToken ct = default)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load KPIs.");
                ErrorMessage = "Unable to load KPIs.";
            }
        }
    }
}
