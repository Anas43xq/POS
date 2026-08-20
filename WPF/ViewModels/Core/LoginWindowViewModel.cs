using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public class LoginWindowViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly IShiftService _shiftService;
        private readonly IRegistryService _registryService;
        private readonly IManagerSessionCache _managerSessionCache;
        private readonly ILogger<LoginWindowViewModel> _logger;

        private bool _hasLoadedRememberedUsername;
        private LoginRole _selectedRole = LoginRole.Cashier;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string? _errorMessage;
        private bool _isBusy;
        private bool _isUsernameReadOnly;
        private bool _focusUsername;
        private bool _focusPassword;
        private bool _isNonManagerRejection;
        private string? _rejectedRole;

        public LoginWindowViewModel(
            IAuthService authService,
            INavigationService navigationService,
            ISessionService sessionService,
            IUserService userService,
            IShiftService shiftService,
            IRegistryService registryService,
            IManagerSessionCache managerSessionCache,
            ILogger<LoginWindowViewModel> logger)
        {
            _authService = authService;
            _navigationService = navigationService;
            _sessionService = sessionService;
            _userService = userService;
            _shiftService = shiftService;
            _registryService = registryService;
            _managerSessionCache = managerSessionCache;
            _logger = logger;

            SelectManagerCommand = new RelayCommand(SelectManagerRole, () => !IsBusy);
            SelectCashierCommand = new RelayCommand(SelectCashierRole, () => !IsBusy);
            LoginCommand = new AsyncRelayCommand(LoginAsManagerAsync, CanLoginAsManager);
            ContinueAsCashierCommand = new AsyncRelayCommand(LoginAsCashierAsync, () => !IsBusy);
            UseDifferentAccountCommand = new AsyncRelayCommand(UseDifferentAccountAsync, () => !IsBusy && IsUsernameReadOnly);
        }

        public event Action? LoginSucceeded;

        public event Action? PasswordResetRequested;

        public ICommand SelectManagerCommand { get; }

        public ICommand SelectCashierCommand { get; }

        public ICommand LoginCommand { get; }

        public ICommand ContinueAsCashierCommand { get; }

        public ICommand UseDifferentAccountCommand { get; }

        public bool IsManagerMode => _selectedRole == LoginRole.Manager;

        public bool IsCashierMode => _selectedRole == LoginRole.Cashier;

        public string Username
        {
            get => _username;
            set
            {
                var nextValue = value ?? string.Empty;
                if (_username == nextValue) return;
                _username = nextValue;
                OnPropertyChanged();
                RaiseCommandStates();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                var nextValue = value ?? string.Empty;
                if (_password == nextValue) return;
                _password = nextValue;
                OnPropertyChanged();
                RaiseCommandStates();
            }
        }

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

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                RaiseCommandStates();
            }
        }

        public bool IsUsernameReadOnly
        {
            get => _isUsernameReadOnly;
            private set
            {
                if (_isUsernameReadOnly == value) return;
                _isUsernameReadOnly = value;
                OnPropertyChanged();
                RaiseCommandStates();
            }
        }

        public bool FocusUsername
        {
            get => _focusUsername;
            private set
            {
                if (_focusUsername == value) return;
                _focusUsername = value;
                OnPropertyChanged();
            }
        }

        public bool FocusPassword
        {
            get => _focusPassword;
            private set
            {
                if (_focusPassword == value) return;
                _focusPassword = value;
                OnPropertyChanged();
            }
        }

        public bool IsNonManagerRejection
        {
            get => _isNonManagerRejection;
            private set
            {
                if (_isNonManagerRejection == value) return;
                _isNonManagerRejection = value;
                OnPropertyChanged();
            }
        }

        public string? RejectedRole
        {
            get => _rejectedRole;
            private set
            {
                if (_rejectedRole == value) return;
                _rejectedRole = value;
                OnPropertyChanged();
            }
        }

        private bool CanLoginAsManager() =>
            !IsBusy
            && IsManagerMode
            && !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrWhiteSpace(Password);

        private void RaiseCommandStates()
        {
            if (SelectManagerCommand is RelayCommand selectManager)
            {
                selectManager.RaiseCanExecuteChanged();
            }

            if (SelectCashierCommand is RelayCommand selectCashier)
            {
                selectCashier.RaiseCanExecuteChanged();
            }

            if (LoginCommand is AsyncRelayCommand login)
            {
                login.RaiseCanExecuteChanged();
            }

            if (ContinueAsCashierCommand is AsyncRelayCommand cashier)
            {
                cashier.RaiseCanExecuteChanged();
            }

            if (UseDifferentAccountCommand is AsyncRelayCommand useDifferentAccount)
            {
                useDifferentAccount.RaiseCanExecuteChanged();
            }
        }

        private void SelectManagerRole()
        {
            if (IsBusy) return;

            _selectedRole = LoginRole.Manager;
            ClearTransientState();
            OnPropertyChanged(nameof(IsManagerMode));
            OnPropertyChanged(nameof(IsCashierMode));
            _ = EnsureManagerModeInitializedAsync();
            RaiseCommandStates();
        }

        private void SelectCashierRole()
        {
            if (IsBusy) return;

            _selectedRole = LoginRole.Cashier;
            ClearTransientState();
            OnPropertyChanged(nameof(IsManagerMode));
            OnPropertyChanged(nameof(IsCashierMode));
            RaiseCommandStates();
        }

        private void ClearTransientState()
        {
            ErrorMessage = null;
            IsNonManagerRejection = false;
            RejectedRole = null;
        }

        private async Task EnsureManagerModeInitializedAsync()
        {
            if (_hasLoadedRememberedUsername) 
            {
                FocusManagerField();
                return;
            }

            _hasLoadedRememberedUsername = true;

            try
            {
                var remembered = await _registryService.GetRememberedUsernameAsync();
                if (string.IsNullOrWhiteSpace(remembered))
                {
                    IsUsernameReadOnly = false;
                    Username = string.Empty;
                }
                else
                {
                    Username = remembered.Trim();
                    IsUsernameReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load remembered username from registry");
                IsUsernameReadOnly = false;
                Username = string.Empty;
            }

            FocusManagerField();
        }

        private void FocusManagerField()
        {
            if (IsUsernameReadOnly)
            {
                RequestPasswordFocus();
            }
            else
            {
                RequestUsernameFocus();
            }
        }

        private void RequestUsernameFocus()
        {
            FocusPassword = false;
            FocusUsername = false;
            FocusUsername = true;
        }

        private void RequestPasswordFocus()
        {
            FocusUsername = false;
            FocusPassword = false;
            FocusPassword = true;
        }

        private async Task UseDifferentAccountAsync()
        {
            if (IsBusy || !IsUsernameReadOnly) return;

            try
            {
                await _registryService.ClearRememberedUsernameAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clear remembered username");
            }

            IsUsernameReadOnly = false;
            Username = string.Empty;
            ClearPassword();
            RequestUsernameFocus();
        }

        private async Task LoginAsManagerAsync()
        {
            if (!CanLoginAsManager()) return;

            IsBusy = true;
            ClearTransientState();
            var stopwatch = Stopwatch.StartNew();
            TxpTrace.WriteLine($"[TXP] - Manager login button pressed for {Username}");

            try
            {
                Result<UserDto> result = await _authService.LoginAsync(Username, Password);
                if (!result.IsSuccess || result.Value is null)
                {
                    ErrorMessage = result.Error ?? "Invalid username or password.";
                    ClearPassword();
                    return;
                }

                var user = result.Value;
                if (!string.Equals(user.RoleName, "Manager", StringComparison.OrdinalIgnoreCase))
                {
                    var actualRole = string.IsNullOrWhiteSpace(user.RoleName) ? "(no role)" : user.RoleName;
                    ErrorMessage =
                        $"This account is a {actualRole} account, not a Manager. " +
                        "Use the Cashier login instead, or sign in with a Manager account.";
                    IsNonManagerRejection = true;
                    RejectedRole = actualRole;
                    var rejectedPreviousId = _sessionService.CurrentUser?.UserId;
                    if (rejectedPreviousId.HasValue)
                        _managerSessionCache.Invalidate(rejectedPreviousId.Value);
                    _sessionService.CurrentUser = null;
                    _sessionService.CurrentShift = null;
                    ClearPassword();

                    _logger.LogWarning(
                        "Rejected manager login: user {Username} has role {Role}, not Manager",
                        user.Username,
                        actualRole);
                    return;
                }

                var previousUserId = _sessionService.CurrentUser?.UserId;
                if (previousUserId.HasValue)
                    _managerSessionCache.Invalidate(previousUserId.Value);
                _sessionService.CurrentUser = user;
                _sessionService.CurrentShift = null;

                await _registryService.SaveRememberedUsernameAsync(user.Username);
                TxpTrace.WriteLine(
                    $"[TXP] - Manager credential validation completed in {stopwatch.ElapsedMilliseconds} ms for {user.Username}");

                LoginSucceeded?.Invoke();
                _ = TryLoadCurrentShiftAsync(user.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager login failed for {Username}", Username);
                ErrorMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoginAsCashierAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ClearTransientState();
            var stopwatch = Stopwatch.StartNew();
            TxpTrace.WriteLine("[TXP] - Cashier role button pressed");

            try
            {
                var defaultCashierStopwatch = Stopwatch.StartNew();
                var cashier = await _userService.GetDefaultCashierAsync();
                TxpTrace.WriteLine(
                    $"[TXP] - Cashier default-user lookup completed in {defaultCashierStopwatch.ElapsedMilliseconds} ms");

                if (cashier is null)
                {
                    ErrorMessage = "No active cashier account was found. Please contact your administrator.";
                    return;
                }

                _sessionService.CurrentUser = cashier;
                _sessionService.CurrentShift = null;
                LoginSucceeded?.Invoke();
                _ = HydrateCashierShiftAfterShellOpenAsync(cashier.Username);

                TxpTrace.WriteLine(
                    $"[TXP] - Cashier login handoff completed in {stopwatch.ElapsedMilliseconds} ms for {cashier.Username}");
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

        private async Task HydrateCashierShiftAfterShellOpenAsync(string username)
        {
            await TryLoadCurrentShiftAsync(username);

            if (_navigationService.CurrentViewModel is CashierDashboardViewModel cashierViewModel)
            {
                await cashierViewModel.RefreshAfterShiftHydrationAsync();
            }
        }

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
                    TxpTrace.WriteLine(
                        $"[TXP] - Hydrated CurrentShift for {username} (ShiftId={openShift.Value.ShiftId})");
                }
                else
                {
                    TxpTrace.WriteLine(
                        $"[TXP] - No open shift for {username}: {openShift.Error ?? "no active shift"}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to load current shift for {Username}; the dashboard will continue without an active shift",
                    username);
            }
        }

        private void ClearPassword()
        {
            Password = string.Empty;
            PasswordResetRequested?.Invoke();
        }

        private enum LoginRole
        {
            Cashier,
            Manager
        }
    }
}
