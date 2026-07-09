using BLL.Interfaces;
using BLL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UI.Commands;
using UI.Services;
using UI.Views;

namespace UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Login-As role-picker window.
    ///
    /// Two role buttons are exposed:
    /// • <see cref="OpenManagerCommand"/> — opens the manager login
    ///   dialog. On success, <see cref="ManagerLoginSucceeded"/> fires
    ///   and the Login-As window closes.
    /// • <see cref="OpenCashierCommand"/> — auto-signs in as the
    ///   default cashier. On success,
    ///   <see cref="CashierLoginSucceeded"/> fires and the Login-As
    ///   window closes.
    ///
    /// The <see cref="ManagerLoginViewModel"/> is intentionally
    /// <em>not</em> a constructor dependency: it is resolved from
    /// the service provider only when the user actually clicks
    /// <c>Manager</c>. This keeps the Windows Registry read (which
    /// happens inside <see cref="ManagerLoginViewModel"/>'s
    /// constructor) off the startup critical path.
    /// </summary>
    public class LoginAsViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly IShiftService _shiftService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LoginAsViewModel> _logger;

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (_errorMessage == value) return;
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>Role the user picked, or <c>null</c> if the window was dismissed.</summary>
        public string? SelectedRole { get; private set; }

        public ICommand OpenManagerCommand { get; }
        public ICommand OpenCashierCommand { get; }

        /// <summary>Fired after a successful manager login.</summary>
        public event Action? ManagerLoginSucceeded;

        /// <summary>Fired after a successful cashier auto-login.</summary>
        public event Action? CashierLoginSucceeded;

        public LoginAsViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ISessionService sessionService,
            IUserService userService,
            IShiftService shiftService,
            IServiceProvider serviceProvider,
            ILogger<LoginAsViewModel> logger)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _sessionService = sessionService;
            _userService = userService;
            _shiftService = shiftService;
            _serviceProvider = serviceProvider;
            _logger = logger;

            OpenManagerCommand = new AsyncRelayCommand(OpenManagerAsync, () => !IsBusy);
            OpenCashierCommand = new AsyncRelayCommand(OpenCashierAsync, () => !IsBusy);
        }

        private void RaiseCanExecuteChanged()
        {
            (OpenManagerCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (OpenCashierCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task OpenManagerAsync()
        {
            if (IsBusy) return;
            SelectedRole = "Manager";
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                // Lazily resolve the manager VM here (not at construction)
                // so the registry read inside its ctor only happens when
                // the user actually opens the dialog. Each Manager click
                // gets a fresh VM → a fresh single registry read.
                var managerVm = _serviceProvider.GetRequiredService<ManagerLoginViewModel>();

                // ManagerLoginDialog subscribes to its VM's
                // LoginSucceeded event in its code-behind and
                // sets DialogResult = true. So a `true` return
                // from ShowDialogWithResult means "manager
                // authenticated successfully".
                var result = _dialogService.ShowDialogWithResult<ManagerLoginDialog>(managerVm);

                if (result == true)
                {
                    // Hydrate the session with any already-open shift for
                    // the manager (the original LoginViewModel did this
                    // after a successful login too).
                    await TryLoadCurrentShiftAsync(managerVm.Username);

                    ManagerLoginSucceeded?.Invoke();
                    CloseLoginAsWindow();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager login dialog failed");
                ErrorMessage = "An error occurred while opening the manager login dialog.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OpenCashierAsync()
        {
            if (IsBusy) return;
            SelectedRole = "Cashier";
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                var cashier = await _userService.GetDefaultCashierAsync();
                if (cashier is null)
                {
                    ErrorMessage =
                        "No active cashier account was found. Please contact your administrator.";
                    return;
                }

                _sessionService.CurrentUser = cashier;
                _logger.LogInformation(
                    "Auto-signed in as cashier {Username} (UserId={UserId})",
                    cashier.Username, cashier.UserId);

                // Hydrate the session with the cashier's already-open
                // shift (if any) so the dashboard renders the correct
                // shift status on first paint.
                await TryLoadCurrentShiftAsync(cashier.Username);

                CashierLoginSucceeded?.Invoke();
                CloseLoginAsWindow();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cashier auto-login failed");
                ErrorMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Asks <see cref="IShiftService"/> for the user's currently
        /// open shift and stores it in <see cref="ISessionService"/>.
        /// Best-effort: a missing or failed lookup leaves
        /// <see cref="ISessionService.CurrentShift"/> at <c>null</c>
        /// so the dashboard can prompt the cashier to start a shift.
        /// </summary>
        private async Task TryLoadCurrentShiftAsync(string username)
        {
            try
            {
                var userId = _sessionService.CurrentUser?.UserId;
                if (userId is null) return;

                var openShift = await _shiftService.GetOpenShiftAsync(userId.Value);
                if (openShift.IsSuccess && openShift.Value is not null)
                {
                    _sessionService.CurrentShift = openShift.Value;
                    _logger.LogInformation(
                        "Hydrated CurrentShift for {Username} (ShiftId={ShiftId})",
                        username, openShift.Value.ShiftId);
                }
                else
                {
                    _logger.LogInformation(
                        "No open shift for {Username}: {Reason}",
                        username,
                        openShift.Error ?? "no active shift");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to load current shift for {Username}; cashier will be prompted to start a shift",
                    username);
            }
        }

        private void CloseLoginAsWindow()
        {
            // Matches the established dialog-close pattern used by
            // StartDayDialogViewModel / EndDayDialogViewModel: find
            // the Window whose DataContext is this VM and close it.
            Application.Current?.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }
    }
}
