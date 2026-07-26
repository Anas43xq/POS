using System;
using System.Threading.Tasks;
using System.Windows;

namespace UI.ViewModels
{
    public partial class MainViewModel
    {
        private ManagerMainViewModel? _currentManagerMainViewModel;
        private HomeViewModel? _currentManagerHomeViewModel;
        private CashierDashboardViewModel? _currentCashierDashboardViewModel;

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

        private void NavigateToDashboardByRole()
        {
            var user = _sessionService.CurrentUser;
            if (string.Equals(user?.RoleName, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                // Navigation triggers OnCurrentViewModelChanged, which is the
                // single source of truth for (un)subscribing the logout bridge.
                // Re-subscribing here would cause EveLogoutRequested to be
                // wired to ManagerMainViewModel.LogoutRequested twice — and
                // therefore the logout MessageBox would show twice on click.
                _navigationService.NavigateTo<ManagerMainViewModel>();
            }
            else
            {
                _navigationService.NavigateTo<CashierDashboardViewModel>();
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
            if (_navigationService.CurrentViewModel is not CashierDashboardViewModel cashierVm)
                return;

            // Same idempotent pattern as SubscribeManagerLogout: cache the
            // current instance so a future UnsubscribeCashierEvents() can
            // detach from the *exact* VM it attached to, even if the
            // navigation service has already moved on to a different view.
            if (_currentCashierDashboardViewModel == cashierVm)
                return;

            UnsubscribeCashierEvents();
            _currentCashierDashboardViewModel = cashierVm;
            _currentCashierDashboardViewModel.LogoutRequested += EveLogoutRequested;
        }

        private void UnsubscribeCashierEvents()
        {
            if (_currentCashierDashboardViewModel == null)
                return;

            _currentCashierDashboardViewModel.LogoutRequested -= EveLogoutRequested;
            _currentCashierDashboardViewModel = null;
        }

        private void UnsubscribeManagerHomeEvents()
        {
            if (_currentManagerHomeViewModel == null)
                return;

            _currentManagerHomeViewModel = null;
        }

        /// <summary>
        /// The user wants the logout MessageBox to live on the
        /// <see cref="MainViewModel"/> (not on the child VM), so
        /// the same prompt is shown no matter which child VM
        /// (manager or cashier) initiated the logout.
        /// </summary>
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

        /// <summary>
        /// Bridge between a child VM's <c>LogoutRequested</c> and
        /// this <see cref="MainViewModel"/>'s <c>LogoutAsync</c>.
        /// The child VM just raises its event; this handler shows
        /// the confirmation MessageBox here (so it lives on the
        /// MainViewModel per the user's request) and, on Yes,
        /// raises <see cref="LogoutRequested"/> for the
        /// <c>ApplicationShellService</c>.
        /// </summary>
        private void EveLogoutRequested()
        {
            _ = LogoutAsync();
        }
    }
}
