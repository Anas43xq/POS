using BLL.Interfaces;
using Contracts.Sales;
using Contracts.Transactions;
using Contracts.Shifts;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly IKpiService _kpiService;
        private readonly IRecentTransactionService _recentTransactionService;
        private readonly IShiftSummaryService _shiftSummaryService;
        private readonly ITopProductService _topProductService;
        private readonly ITransactionService _transactionService;
        private readonly IReceiptDisplayService _receiptDisplayService;
        private string _errorMessage = string.Empty;
        public event Action? ViewAllShiftsRequested;
        public event Action? ShowAllTransactionsRequested;

        public HomeViewModel(
            IKpiService kpiService,
            IRecentTransactionService recentTransactionService,
            IShiftSummaryService shiftSummaryService,
            ITopProductService topProductService,
            ITransactionService transactionService,
            IReceiptDisplayService receiptDisplayService)
        {
            _kpiService = kpiService;
            _recentTransactionService = recentTransactionService;
            _shiftSummaryService = shiftSummaryService;
            _topProductService = topProductService;
            _transactionService = transactionService;
            _receiptDisplayService = receiptDisplayService;

            FilterTodayCommand = new RelayCommand(_ => OnFilterToday());
            FilterThisWeekCommand = new RelayCommand(_ => OnFilterThisWeek());
            FilterMonthCommand = new RelayCommand(_ => OnFilterThisMonth());
            ShowPeriodFilterCommand = new RelayCommand(_ => OnShowPeriodFilter());
            ApplyPeriodFilterCommand = new RelayCommand(_ => OnApplyPeriodFilter());
            OpenReceiptCommand = new RelayCommand<RecentTransactionDto>(OpenReceipt);
            OpenShiftManagementCommand = new RelayCommand(OpenShiftManagement);
            ShowAllTransactionsCommand = new RelayCommand(ShowAllTransactions);
            RefreshCommand = new AsyncRelayCommand(RefreshDataAsync);

            CurrentFilterMode = DashboardFilterMode.Today;
            _ = LoadAsyncSafe();
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                _errorMessage = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

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
            catch (Exception)
            {
                ErrorMessage = "Unable to load dashboard data.";
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalSales;
        public decimal TotalSales
        {
            get => _totalSales;
            private set { _totalSales = value; OnPropertyChanged(); }
        }

        private decimal _averageSale;
        public decimal AverageSales
        {
            get => _averageSale;
            private set { _averageSale = value; OnPropertyChanged(); }
        }

        private decimal _totalCash;
        public decimal TotalCash
        {
            get => _totalCash;
            private set { _totalCash = value; OnPropertyChanged(); }
        }

        private decimal _totalCard;
        public decimal TotalCard
        {
            get => _totalCard;
            private set { _totalCard = value; OnPropertyChanged(); }
        }

        private int _totalOrders;
        public int TotalOrders
        {
            get => _totalOrders;
            private set { _totalOrders = value; OnPropertyChanged(); }
        }

        private DashboardFilterMode _currentFilterMode;
        public DashboardFilterMode CurrentFilterMode
        {
            get => _currentFilterMode;
            private set
            {
                if (_currentFilterMode == value)
                    return;

                _currentFilterMode = value;
                OnPropertyChanged();
            }
        }

        public ICommand FilterTodayCommand { get; }
        public ICommand FilterThisWeekCommand { get; }
        public ICommand FilterMonthCommand { get; }
        public ICommand ShowPeriodFilterCommand { get; }
        public ICommand ApplyPeriodFilterCommand { get; }

        private bool _isPeriodFilterVisible;
        public bool IsPeriodFilterVisible
        {
            get => _isPeriodFilterVisible;
            set
            {
                if (_isPeriodFilterVisible == value)
                    return;

                _isPeriodFilterVisible = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _periodFromDate;
        public DateTime? PeriodFromDate
        {
            get => _periodFromDate;
            set
            {
                if (_periodFromDate == value)
                    return;

                _periodFromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _periodToDate;
        public DateTime? PeriodToDate
        {
            get => _periodToDate;
            set
            {
                if (_periodToDate == value)
                    return;

                _periodToDate = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<object> _topProducts = new();
        public ObservableCollection<object> TopProducts
        {
            get => _topProducts;
            set
            {
                if (_topProducts != value)
                {
                    _topProducts = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasTopProducts));
                    OnPropertyChanged(nameof(HasNoTopProducts));
                }
            }
        }

        public bool HasTopProducts => TopProducts.Count > 0;
        public bool HasNoTopProducts => !HasTopProducts;

        private ObservableCollection<Contracts.Shifts.ShiftSummaryView> _shiftSummaries = new();
        public ObservableCollection<Contracts.Shifts.ShiftSummaryView> ShiftSummaries
        {
            get => _shiftSummaries;
            set
            {
                if (_shiftSummaries != value)
                {
                    _shiftSummaries = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasShiftSummaries));
                    OnPropertyChanged(nameof(HasNoShiftSummaries));
                }
            }
        }

        public bool HasShiftSummaries => ShiftSummaries.Count > 0;
        public bool HasNoShiftSummaries => !HasShiftSummaries;

        private ObservableCollection<RecentTransactionDto> _recentTransactions = new();
        public ObservableCollection<RecentTransactionDto> RecentTransactions
        {
            get => _recentTransactions;
            set
            {
                if (_recentTransactions != value)
                {
                    _recentTransactions = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasRecentTransactions));
                    OnPropertyChanged(nameof(HasNoRecentTransactions));
                }
            }
        }

        public bool HasRecentTransactions => RecentTransactions.Count > 0;
        public bool HasNoRecentTransactions => !HasRecentTransactions;

        public ICommand OpenReceiptCommand { get; }
        public ICommand OpenShiftManagementCommand { get; }
        public ICommand ShowAllTransactionsCommand { get; }
        public ICommand RefreshCommand { get; }

        public async Task LoadAsync()
        {
            await LoadAsyncCore();
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                await LoadAsyncCore();
                ErrorMessage = string.Empty;
            }
            catch (Exception)
            {
                ErrorMessage = "Unable to refresh dashboard data.";
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
            catch (Exception)
            {
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
            catch (Exception)
            {
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
            catch (Exception)
            {
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

    public enum DashboardFilterMode
    {
        Today,
        Week,
        Month,
        Period
    }
}
