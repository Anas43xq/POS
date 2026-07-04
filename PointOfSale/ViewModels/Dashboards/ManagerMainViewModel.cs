using BLL.Interfaces;
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
    public class ManagerMainViewModel : BaseViewModel
    {
        private readonly HomeViewModel _homeViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;
        private readonly ShiftManagementViewModel _shiftManagementViewModel;
        private readonly ReportViewModel _reportViewModel;
        private readonly ProductManagementViewModel _productManagementViewModel;
        private readonly CategoryManagementViewModel _categoryManagementViewModel;
        private readonly ISessionService _sessionService;
        private CancellationTokenSource? _timeCancellationTokenSource;

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
        public ICommand LogoutCommand { get; set; } = null!;

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
            ISessionService sessionService)
        {
            _homeViewModel = homeViewModel;
            _transactionsViewModel = transactionsViewModel;
            _shiftManagementViewModel = shiftManagementViewModel;
            _reportViewModel = reportViewModel;
            _productManagementViewModel = productManagementViewModel;
            _categoryManagementViewModel = categoryManagementViewModel;
            _sessionService = sessionService;

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
            LogoutCommand = new RelayCommand(_ => LogoutRequested?.Invoke());

            // Initialize manager info
            InitializeManagerInfo();

            // Initialize with HomeViewModel (LOCAL property, not global service)
            CurrentPage = _homeViewModel;
        }

        private void UpdateCurrentDateTime()
        {
            CurrentDateTime = !string.IsNullOrWhiteSpace(CurrentDayName) && !string.IsNullOrWhiteSpace(CurrentDate) && !string.IsNullOrWhiteSpace(CurrentTime)
                ? $"{CurrentDayName}, {CurrentDate}   {CurrentTime}"
                : string.Empty;
        }

        private void NavigateToTransactions()
        {
            CurrentPage = _transactionsViewModel;
            _ = _transactionsViewModel.RefreshAsync();
        }

        private void NavigateToShiftManagement()
        {
            CurrentPage = _shiftManagementViewModel;
            _ = _shiftManagementViewModel.RefreshAsync();
        }

        private void NavigateToReports()
        {
            CurrentPage = _reportViewModel;
        }

        private void NavigateToProductManagement()
        {
            CurrentPage = _productManagementViewModel;
        }

        private void NavigateToCategoryManagement()
        {
            CurrentPage = _categoryManagementViewModel;
        }

        /// <summary>
        /// Initialize manager information from session.
        /// </summary>
        private void InitializeManagerInfo()
        {
            var currentUser = _sessionService.CurrentUser;
            ManagerName = currentUser?.FullName ?? "Manager";
            CurrentDayName = DateTime.Now.ToString("dddd");
            CurrentDate = DateTime.Now.ToString("MMMM dd, yyyy");
            CurrentTime = DateTime.Now.ToString("hh:mm tt");
            UpdateCurrentDateTime();
            _ = UpdateTimeAsync();
        }

        /// <summary>
        /// Update current time every minute.
        /// </summary>
        private async Task UpdateTimeAsync()
        {
            try
            {
                _timeCancellationTokenSource = new CancellationTokenSource();
                while (!_timeCancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), _timeCancellationTokenSource.Token);
                    CurrentTime = DateTime.Now.ToString("hh:mm tt");
                }
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled, exit gracefully
            }
        }

    }
}
