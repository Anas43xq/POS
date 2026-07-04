using BLL.Interfaces;
using Contracts.Sales;
using Contracts.Transactions;
using DAL.Entities;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UI.Commands;
using UI.Services;
using UI.Views;

namespace UI.ViewModels;

public class MainViewModel : BaseViewModel
    {
        private readonly CashierDashboardViewModel _cashierDashboardViewModel;
        private readonly INavigationService _navigationService;
        private readonly ISessionService _sessionService;

         public event Action? LogoutRequested;

        /// <summary>
        /// Exposes CurrentViewModel from NavigationService as read-only.
        /// </summary>
        public object? CurrentViewModel => _navigationService.CurrentViewModel;

        public bool IsCashierView => _navigationService.CurrentViewModel is CashierDashboardViewModel;

        public bool IsManagerView => _navigationService.CurrentViewModel is ManagerMainViewModel;



    public MainViewModel(
        CashierDashboardViewModel cashierDashboardViewModel,
        INavigationService navigationService,
        ITransactionService transactionService,
        ISessionService sessionService,
        IDialogService dialogService
    )
    {
        _cashierDashboardViewModel = cashierDashboardViewModel;
        _navigationService = navigationService;
        _sessionService = sessionService;

        // Subscribe to navigation changes and refresh UI
        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

        // Navigate to dashboard based on user role
        NavigateToDashboardByRole();
    }

    /// <summary>
    /// Called when the current ViewModel changes in NavigationService.
    /// Updates bindings so UI reflects the new ViewModel.
    /// </summary>
    private ManagerMainViewModel? _currentManagerMainViewModel;

private HomeViewModel? _currentManagerHomeViewModel;

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(IsCashierView));
            OnPropertyChanged(nameof(IsManagerView));

            if (_navigationService.CurrentViewModel is ManagerMainViewModel managerViewModel)
            {
                SubscribeManagerLogout(managerViewModel);
                SubscribeManagerHomeEvents(managerViewModel);
            }
            else if (_navigationService.CurrentViewModel is CashierDashboardViewModel)
            {
                UnsubscribeManagerLogout();
                UnsubscribeManagerHomeEvents();
                SubscribeCashierEvents();
            }
            else
            {
                UnsubscribeManagerLogout();
                UnsubscribeManagerHomeEvents();
            }
    }

    /// <summary>
    /// Navigates to the appropriate dashboard based on the current user's role.
    /// Manager roles navigate to ManagerMainViewModel, others to CashierDashboardViewModel.
    /// </summary>
    private void NavigateToDashboardByRole()
    {
        try
        {
            var user = _sessionService.CurrentUser;
            if (user?.Role != null && user.Role.RoleName.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            {
                _navigationService.NavigateTo<ManagerMainViewModel>();
                if (_navigationService.CurrentViewModel is ManagerMainViewModel managerViewModel)
                {
                    SubscribeManagerLogout(managerViewModel);
                }
            }
            else
            {
                _navigationService.NavigateTo<CashierDashboardViewModel>();
                SubscribeCashierEvents();
            }
        }
        catch
        {
            throw;
        }
    }

    private void SubscribeManagerLogout(ManagerMainViewModel managerViewModel)
    {
        if (_currentManagerMainViewModel == managerViewModel)
            return;

        UnsubscribeManagerLogout();
        _currentManagerMainViewModel = managerViewModel;
        _currentManagerMainViewModel.LogoutRequested += EveLogoutRequested;
    }

    private void SubscribeManagerHomeEvents(ManagerMainViewModel managerViewModel)
    {
        if (_currentManagerHomeViewModel == managerViewModel.HomeVM)
            return;

        UnsubscribeManagerHomeEvents();
        _currentManagerHomeViewModel = managerViewModel.HomeVM;
    }

    private void UnsubscribeManagerLogout()
    {
        if (_currentManagerMainViewModel == null)
            return;

        _currentManagerMainViewModel.LogoutRequested -= EveLogoutRequested;
        _currentManagerMainViewModel = null;
    }

    private void SubscribeCashierEvents()
    {
        // Unsubscribe from old instance first
        UnsubscribeCashierEvents();

        // Subscribe to the NavigationService instance (the one actually displayed)
        if (_navigationService.CurrentViewModel is CashierDashboardViewModel cashierVm)
        {
            cashierVm.LogoutRequested += EveLogoutRequested;
        }
    }

    private void UnsubscribeCashierEvents()
    {
        if (_navigationService.CurrentViewModel is CashierDashboardViewModel cashierVm)
        {
            cashierVm.LogoutRequested -= EveLogoutRequested;
        }
    }

    private void UnsubscribeManagerHomeEvents()
    {
        if (_currentManagerHomeViewModel == null)
            return;

        _currentManagerHomeViewModel = null;
    }

    private async Task LogoutAsync()
    {
        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to logout?",
            "Confirm Logout",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        _sessionService.CurrentUser = null;

        LogoutRequested?.Invoke();

        await Task.CompletedTask;
    }

    private void EveLogoutRequested()
    {
        _ = LogoutAsync();
    }
}

