using BLL.Interfaces;
using Contracts.Enum;
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
using UI.Services;

namespace UI.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly ILogger<HomeViewModel>? _logger;
        private readonly IKpiService _kpiService;
        private readonly IRecentTransactionService _recentTransactionService;
        private readonly IShiftSummaryService _shiftSummaryService;
        private readonly ITopProductService _topProductService;
        private readonly ITransactionService _transactionService;
        private readonly IReceiptDisplayService _receiptDisplayService;
        private readonly IKeyboardShortcutService _shortcutService;
        private string _errorMessage = string.Empty;
        private bool _isInitialLoadBusy;
        public event Action? ViewAllShiftsRequested;
        public event Action? ShowAllTransactionsRequested;

        public string RefreshGesture => GetShortcutGesture(ShortcutAction.Refresh);

        public HomeViewModel(
            IKpiService kpiService,
            IRecentTransactionService recentTransactionService,
            IShiftSummaryService shiftSummaryService,
            ITopProductService topProductService,
            ITransactionService transactionService,
            IReceiptDisplayService receiptDisplayService,
            IKeyboardShortcutService shortcutService,
            ILogger<HomeViewModel>? logger)
        {
            _logger = logger;
            _kpiService = kpiService;
            _recentTransactionService = recentTransactionService;
            _shiftSummaryService = shiftSummaryService;
            _topProductService = topProductService;
            _transactionService = transactionService;
            _receiptDisplayService = receiptDisplayService;
            _shortcutService = shortcutService;

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

        private ObservableCollection<TopProductDto> _topProducts = new();
        public ObservableCollection<TopProductDto> TopProducts
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
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to refresh dashboard data.");
                ErrorMessage = "Unable to refresh dashboard data.";
            }
        }

        private string GetShortcutGesture(ShortcutAction action)
        {
            var bindings = _shortcutService.GetActiveBindings();
            var binding = bindings.FirstOrDefault(b => b.Action == action);
            return binding?.KeyGesture ?? string.Empty;
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
