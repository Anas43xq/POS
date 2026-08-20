using BLL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    /// <summary>
    /// Drives the manager PIN override popup.
    /// Callers await <see cref="ResultTask"/> to get the approval outcome.
    /// </summary>
    public class ManagerPinOverlayViewModel : BaseViewModel
    {
        private readonly IPinService _pinService;
        private readonly ISessionService _sessionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<ManagerPinOverlayViewModel> _logger;
        private readonly INotificationService _notifications;
        private readonly TaskCompletionSource<bool> _tcs = new();
        private readonly TaskCompletionSource<string?> _reasonTcs = new();

        private string _pin = string.Empty;
        private string? _errorMessage;
        private bool _isBusy;
        private bool _isPinSet;
        private int _failedAttempts;
        private string _reason = string.Empty;

        private const int MaxAttempts = 5;

        public ManagerPinOverlayViewModel(
            IPinService pinService,
            ISessionService sessionService,
            ILocalizationService localizationService,
            ILogger<ManagerPinOverlayViewModel> logger,
            INotificationService notifications)
        {
            _pinService = pinService;
            _sessionService = sessionService;
            _localizationService = localizationService;
            _logger = logger;
            _notifications = notifications;

            DigitCommand   = new RelayCommand(OnDigit,   _ => !IsBusy && IsPinSet && _pin.Length < 4);
            BackspaceCommand = new RelayCommand(_ => OnBackspace(), _ => !IsBusy && IsPinSet && _pin.Length > 0);
            ConfirmCommand = new AsyncRelayCommand(OnConfirmAsync, () => !IsBusy && IsPinSet && _pin.Length == 4);
            CancelCommand  = new RelayCommand(_ => OnCancel(), _ => !IsBusy);
        }

        /// <summary>Awaited by the caller to get the approval result.</summary>
        public Task<bool> ResultTask => _tcs.Task;

        public Task<string?> ResultWithReasonTask => _reasonTcs.Task;

        /// <summary>Context label shown in the dialog header (e.g. "Approve Void").</summary>
        public string? PromptTitle { get; set; }

        /// <summary>Whether the caller requires a reason before approval is granted.</summary>
        public bool ReasonRequired { get; set; }

        /// <summary>Reason text entered by the manager.</summary>
        public string Reason
        {
            get => _reason;
            set
            {
                if (_reason == value) return;
                _reason = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public ICommand DigitCommand   { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand  { get; }

        /// <summary>Dot indicators (0–4 filled dots) bound in the XAML.</summary>
        public int PinLength => _pin.Length;

        /// <summary>True when the current manager has a PIN set. When false the
        /// keypad is disabled and a "set a PIN in Settings" notice is shown.</summary>
        public bool IsPinSet => _isPinSet;

        /// <summary>True when no PIN is set — drives the banner and centered notice.</summary>
        public bool ShowPinNotice => !_isPinSet;

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

        public bool HasError => !string.IsNullOrWhiteSpace(_errorMessage);

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                RaiseAll();
            }
        }

        public event Action? CloseRequested;

        /// <summary>Raised after a failed attempt so the code-behind can clear visual digit state.</summary>
        public event Action? PinClearRequested;

        /// <summary>
        /// Checks whether the logged-in manager has a PIN set and flips
        /// <see cref="IsPinSet"/> accordingly. When no PIN exists the keypad is
        /// disabled and a notice directs the manager to Settings. Must be called
        /// before the overlay is shown. Fast after login because the PIN hash is
        /// already cached by <see cref="IPinService.HydrateCacheAsync"/>.
        /// </summary>
        public async Task InitializeAsync()
        {
            var userId = _sessionService.CurrentUser?.UserId;
            _isPinSet = userId is int id && await _pinService.HasPinAsync(id);

            OnPropertyChanged(nameof(IsPinSet));
            OnPropertyChanged(nameof(ShowPinNotice));
            RaiseAll();
        }

        // TODO: long-term fix — change DigitCommand to AsyncRelayCommand so auto-submit can use a proper
        // async pipeline instead of async void. Deferred because the existing pattern uses RelayCommand
        // for this command and a full ViewModel refactor is out of scope. The async void here is
        // sanctioned for ICommand.Execute per the wpf-performance skill: exceptions propagate to the
        // app's UnhandledException handler.
        private async void OnDigit(object? parameter)
        {
            if (!IsPinSet || _pin.Length >= 4 || IsBusy) return;
            _pin += parameter?.ToString() ?? string.Empty;
            ErrorMessage = null;
            OnPropertyChanged(nameof(PinLength));
            RaiseAll();

            if (_pin.Length == 4)
                await OnConfirmAsync();
        }

        private void OnBackspace()
        {
            if (_pin.Length == 0 || IsBusy) return;
            _pin = _pin[..^1];
            ErrorMessage = null;
            OnPropertyChanged(nameof(PinLength));
            RaiseAll();
        }

        private async Task OnConfirmAsync()
        {
            if (!IsPinSet || _pin.Length != 4 || IsBusy) return;

            if (ReasonRequired && string.IsNullOrWhiteSpace(Reason))
            {
                ErrorMessage = _localizationService.GetString("ManagerPin.ErrorReasonRequired");
                _pin = string.Empty;
                OnPropertyChanged(nameof(PinLength));
                PinClearRequested?.Invoke();
                RaiseAll();
                return;
            }

            IsBusy = true;
            try
            {
                var userId = _sessionService.CurrentUser?.UserId;
                if (userId is null)
                {
                    ErrorMessage = _localizationService.GetString("ManagerPin.ErrorNoSession");
                    _tcs.TrySetResult(false);
                    _reasonTcs.TrySetResult(null);
                    CloseRequested?.Invoke();
                    return;
                }

                if (!await _pinService.HasPinAsync(userId.Value))
                {
                    _notifications.ShowWarning(
                        _localizationService.GetString("ManagerPin.ErrorNoPin"));
                    _tcs.TrySetResult(false);
                    _reasonTcs.TrySetResult(null);
                    CloseRequested?.Invoke();
                    return;
                }

                bool ok = await _pinService.VerifyPinAsync(userId.Value, _pin);
                _pin = string.Empty;
                OnPropertyChanged(nameof(PinLength));

                if (ok)
                {
                    _tcs.TrySetResult(true);
                    _reasonTcs.TrySetResult(Reason.Trim());
                    CloseRequested?.Invoke();
                }
                else
                {
                    _failedAttempts++;
                    _logger.LogWarning(
                        "PIN override failed attempt {Attempt}/{Max} for user {UserId}",
                        _failedAttempts, MaxAttempts, userId);

                    PinClearRequested?.Invoke();

                    if (_failedAttempts >= MaxAttempts)
                    {
                        ErrorMessage = _localizationService.GetString("ManagerPin.ErrorLocked");
                        _tcs.TrySetResult(false);
                        _reasonTcs.TrySetResult(null);
                        CloseRequested?.Invoke();
                    }
                    else
                    {
                        ErrorMessage = _localizationService.GetString(
                            "ManagerPin.ErrorIncorrect",
                            MaxAttempts - _failedAttempts);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during PIN verification");
                ErrorMessage = _localizationService.GetString("ManagerPin.ErrorUnexpected");
                PinClearRequested?.Invoke();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnCancel()
        {
            _tcs.TrySetResult(false);
            _reasonTcs.TrySetResult(null);
            CloseRequested?.Invoke();
        }

        private void RaiseAll()
        {
            if (DigitCommand is RelayCommand d)       d.RaiseCanExecuteChanged();
            if (BackspaceCommand is RelayCommand b)   b.RaiseCanExecuteChanged();
            if (ConfirmCommand is AsyncRelayCommand c) c.RaiseCanExecuteChanged();
            if (CancelCommand is RelayCommand x)      x.RaiseCanExecuteChanged();
        }
    }
}
