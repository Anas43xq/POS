using BLL.Interfaces;
using BLL.Models;
using BLL.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Manager Login dialog.
    ///
    /// Responsibilities:
    /// • On construction, pre-fill the <see cref="Username"/> from
    ///   the registry (if a previous successful login remembered it).
    ///   When a remembered username is present, the field is locked
    ///   and focus jumps straight to the password. This registry read
    ///   happens exactly once per dialog open.
    /// • Validate credentials against the existing <see cref="IAuthService"/>
    ///   (which already compares against BCrypt hashes in the database).
    /// • Authorize: even if credentials are valid, only users whose
    ///   role is <c>Manager</c> are allowed to pass. Non-manager
    ///   accounts (e.g. a Cashier) are rejected with a specific,
    ///   actionable error.
    /// • On success, raise <see cref="LoginSucceeded"/> and persist the
    ///   username via <see cref="IRegistryService"/>. The password is
    ///   never persisted.
    /// </summary>
    public class ManagerLoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly IRegistryService _registryService;
        private readonly ILogger<ManagerLoginViewModel> _logger;

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                if (_username == value) return;
                _username = value ?? string.Empty;
                OnPropertyChanged();
                RaiseLoginCanExecuteChanged();
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (_password == value) return;
                _password = value ?? string.Empty;
                OnPropertyChanged();
                RaiseLoginCanExecuteChanged();
            }
        }

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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged();
                RaiseLoginCanExecuteChanged();
            }
        }

        /// <summary>
        /// True only when the username field was auto-loaded from the
        /// registry. The TextBox is then read-only so the user doesn't
        /// accidentally change which account they are signing in as.
        /// </summary>
        public bool IsUsernameReadOnly { get; private set; }

        /// <summary>True when initial focus should land on the username field.</summary>
        public bool FocusUsername { get; private set; }

        /// <summary>True when initial focus should land on the password field.</summary>
        public bool FocusPassword { get; private set; }

        /// <summary>
        /// When the most recent failed login was rejected because
        /// the account is not a Manager, this holds the actual
        /// role name (e.g. <c>"Cashier"</c>) so the UI can suggest
        /// the correct path. <c>null</c> otherwise.
        /// </summary>
        public string? RejectedRole { get; private set; }

        /// <summary>
        /// True when the most recent failure was a role mismatch
        /// (i.e. credentials were valid but the user is not a
        /// manager). The UI can use this to render a "Switch to
        /// Cashier" shortcut.
        /// </summary>
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
        private bool _isNonManagerRejection;

        /// <summary>Raised after a successful manager login. The dialog's owner should close.</summary>
        public event Action<UserDto>? LoginSucceeded;

        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }

        public ManagerLoginViewModel(
            IAuthService authService,
            ISessionService sessionService,
            IRegistryService registryService,
            ILogger<ManagerLoginViewModel> logger)
        {
            _authService = authService;
            _sessionService = sessionService;
            _registryService = registryService;
            _logger = logger;

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            CancelCommand = new RelayCommand(Cancel);

            // Registry read happens exactly once per dialog open,
            // on construction. The dialog is created on demand from
            // LoginAsViewModel.OpenManagerAsync, so this never runs
            // at app startup or while the Login-As window is idle.
            _ = LoadRememberedUsernameAsync();
        }

        private void RaiseLoginCanExecuteChanged()
        {
            if (LoginCommand is AsyncRelayCommand async)
                async.RaiseCanExecuteChanged();
        }

        private bool CanLogin() =>
            !IsLoading
            && !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrEmpty(Password);

        private async Task LoadRememberedUsernameAsync()
        {
            try
            {
                var remembered = await _registryService.GetRememberedUsernameAsync();
                if (string.IsNullOrWhiteSpace(remembered))
                {
                    FocusUsername = true;
                    OnPropertyChanged(nameof(FocusUsername));
                }
                else
                {
                    Username = remembered;
                    IsUsernameReadOnly = true;
                    OnPropertyChanged(nameof(IsUsernameReadOnly));
                    FocusPassword = true;
                    OnPropertyChanged(nameof(FocusPassword));
                }
            }
            catch (Exception ex)
            {
                // Best-effort — if registry load fails, fall back to
                // focusing the username field as if nothing was remembered.
                _logger.LogWarning(ex, "Failed to load remembered username from registry");
                FocusUsername = true;
                OnPropertyChanged(nameof(FocusUsername));
            }
        }

        private async Task LoginAsync()
        {
            if (!CanLogin()) return;

            IsLoading = true;
            ErrorMessage = null;
            IsNonManagerRejection = false;
            RejectedRole = null;

            try
            {
                Result<UserDto> result = await _authService.LoginAsync(Username, Password);

                if (!result.IsSuccess || result.Value is null)
                {
                    ErrorMessage = result.Error ?? "Invalid username or password.";
                    Password = string.Empty;
                    return;
                }

                var user = result.Value;

                // ── Authorization ────────────────────────────────────────────
                // Credentials are valid, but only Manager-role users may
                // pass through the Manager login. Reject everything else
                // with an actionable message that names the actual role
                // and points the user at the Cashier button.
                if (!string.Equals(user.RoleName, "Manager", StringComparison.OrdinalIgnoreCase))
                {
                    var actualRole = string.IsNullOrWhiteSpace(user.RoleName) ? "(no role)" : user.RoleName;

                    ErrorMessage =
                        $"This account is a {actualRole} account, not a Manager. " +
                        "Please use the Cashier button on the previous screen, " +
                        "or sign in with a Manager account.";

                    IsNonManagerRejection = true;
                    RejectedRole = actualRole;

                    _logger.LogWarning(
                        "Rejected manager login: user {Username} has role {Role}, not Manager",
                        user.Username,
                        actualRole);

                    // Wipe the password and clear the session — the user
                    // is back to a clean state and can retry, switch
                    // roles, or cancel.
                    Password = string.Empty;
                    _sessionService.CurrentUser = null;
                    return;
                }

                _sessionService.CurrentUser = user;

                // Persist the username for the next launch. Best-effort.
                await _registryService.SaveRememberedUsernameAsync(user.Username);

                LoginSucceeded?.Invoke(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager login failed for {Username}", Username);
                ErrorMessage = "An unexpected error occurred. Please try again.";
                IsNonManagerRejection = false;
                RejectedRole = null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            // The dialog owner closes on its own; this is a no-op
            // command so the XAML can bind a Cancel button.
        }
    }
}
