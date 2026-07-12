using BLL.Interfaces;
using Contracts.Enum;
using Contracts.Sales;
using Contracts.Transactions;
using POS.Contracts.Receipts;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UI.Commands;
using UI.Services;
using UI.ViewModels;
using UI.Views;


namespace UI.ViewModels
{
    public partial class ManagerMainViewModel : BaseViewModel
    {
        private readonly HomeViewModel _homeViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;
        private readonly ShiftManagementViewModel _shiftManagementViewModel;
        private readonly ReportViewModel _reportViewModel;
        private readonly ProductManagementViewModel _productManagementViewModel;
        private readonly CategoryManagementViewModel _categoryManagementViewModel;
        private readonly SizeManagementViewModel _sizeManagementViewModel;
        private readonly ReceiptManagementViewModel _receiptManagementViewModel;
        private readonly ISessionService _sessionService;
        private readonly IDialogService _dialogService;
        private CancellationTokenSource? _timeCancellationTokenSource;
        private object? _receiptFormPage;

        private string _managerName = string.Empty;
        private string _currentDayName = string.Empty;
        private string _currentDate = string.Empty;
        private string _currentTime = string.Empty;
        private string _currentDateTime = string.Empty;
        private object? _currentPage = null;

        public HomeViewModel HomeVM => _homeViewModel;
        public ICommand NavigateHomeCommand { get; set; } = null!;
        public ICommand NavigateTransactionsCommand { get; set; } = null!;
        public ICommand NavigateShiftManagementCommand { get; set; } = null!;
        public ICommand NavigateReportsCommand { get; set; } = null!;
        public ICommand NavigateProductManagementCommand { get; set; } = null!;
        public ICommand NavigateCategoryManagementCommand { get; set; } = null!;
        public ICommand NavigateSizeManagementCommand { get; set; } = null!;
        public ICommand NavigateReceiptManagementCommand { get; set; } = null!;
        public ICommand LogoutCommand { get; set; } = null!;

        /// <summary>
        /// Opens the Settings dialog (same dialog the cashier header uses).
        /// Mirrors <c>CashierDashboardViewModel.ShowSetting</c>.
        /// </summary>
        public ICommand ShowSetting { get; set; } = null!;

        public string ManagerName
        {
            get => _managerName;
            set
            {
                _managerName = value;
                OnPropertyChanged();
            }
        }

        public string CurrentDayName
        {
            get => _currentDayName;
            set
            {
                _currentDayName = value;
                OnPropertyChanged();
                UpdateCurrentDateTime();
            }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged();
                UpdateCurrentDateTime();
            }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
                UpdateCurrentDateTime();
            }
        }

        public string CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                _currentDateTime = value;
                OnPropertyChanged();
            }
        }

        public event Action? LogoutRequested;

        /// <summary>
        /// Local CurrentPage property for sidebar navigation within ManagerMainView only.
        /// </summary>
        public object? CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public ManagerMainViewModel(
            HomeViewModel homeViewModel,
            TransactionsViewModel transactionsViewModel,
            ShiftManagementViewModel shiftManagementViewModel,
            ReportViewModel reportViewModel,
            ProductManagementViewModel productManagementViewModel,
            CategoryManagementViewModel categoryManagementViewModel,
            SizeManagementViewModel sizeManagementViewModel,
            ReceiptManagementViewModel receiptManagementViewModel,
            ISessionService sessionService,
            IDialogService dialogService)
        {
            _homeViewModel = homeViewModel;
            _transactionsViewModel = transactionsViewModel;
            _shiftManagementViewModel = shiftManagementViewModel;
            _reportViewModel = reportViewModel;
            _productManagementViewModel = productManagementViewModel;
            _categoryManagementViewModel = categoryManagementViewModel;
            _sizeManagementViewModel = sizeManagementViewModel;
            _receiptManagementViewModel = receiptManagementViewModel;
            _sessionService = sessionService;
            _dialogService = dialogService;

            _homeViewModel.ViewAllShiftsRequested -= NavigateToShiftManagement;
            _homeViewModel.ShowAllTransactionsRequested -= NavigateToTransactions;

            _homeViewModel.ViewAllShiftsRequested += NavigateToShiftManagement;
            _homeViewModel.ShowAllTransactionsRequested += NavigateToTransactions;

            // Initialize sidebar navigation commands (LOCAL navigation, not global service)
            NavigateHomeCommand = new RelayCommand(_ => CurrentPage = _homeViewModel);
            NavigateTransactionsCommand = new RelayCommand(NavigateToTransactions);
            NavigateShiftManagementCommand = new RelayCommand(NavigateToShiftManagement);
            NavigateReportsCommand = new RelayCommand(NavigateToReports);
            NavigateProductManagementCommand = new RelayCommand(NavigateToProductManagement);
            NavigateCategoryManagementCommand = new RelayCommand(NavigateToCategoryManagement);
            NavigateSizeManagementCommand = new RelayCommand(NavigateToSizeManagement);
            NavigateReceiptManagementCommand = new RelayCommand(NavigateToReceiptManagement);
            LogoutCommand = new RelayCommand(_ => LogoutRequested?.Invoke());

            // Settings — mirrors the cashier header's gear button so the
            // manager can also change the UI language.
            ShowSetting = new AsyncRelayCommand(OpenSetting);

            // Subscribe to receipt navigation events
            _receiptManagementViewModel.NavigateToFormRequested += OnReceiptNavigateToForm;
            _receiptManagementViewModel.NavigateToListRequested += OnReceiptNavigateToList;

            // Initialize manager info
            InitializeManagerInfo();

            // Initialize with HomeViewModel (LOCAL property, not global service)
            CurrentPage = _homeViewModel;
        }
    }
}
