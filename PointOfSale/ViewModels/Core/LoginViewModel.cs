using BLL.Interfaces;
using BLL.Models;
using BLL.DTOs;
using Microsoft.Extensions.Logging;
using POS.Contracts.Localization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _session;
        private readonly IShiftService _shiftService;
        private readonly IDialogService _dialogService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<LoginViewModel> _logger;
        private string _password = string.Empty;

        public ObservableCollection<LanguageDto> SupportedLanguages { get; }

        private LanguageDto? _selectedLanguage;
        public LanguageDto? SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (value is not null && value.Code != _selectedLanguage?.Code)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();
                    _ = _localizationService.SetLanguageAsync(value.Code);
                }
            }
        }

        public event Action? LoginSucceeded;

        public ICommand LoginCommand { get; }

        private string _username = string.Empty;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                RaiseLoginCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                RaiseLoginCanExecuteChanged();
            }
        }

        private void RaiseLoginCanExecuteChanged()
        {
            if (LoginCommand is AsyncRelayCommand asyncCommand)
            {
                asyncCommand.RaiseCanExecuteChanged();
            }
        }

        private string? _errormessage;
        public string? ErrorMessage
        {
            get => _errormessage;
            set
            {
                _errormessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoginbtnStatus));
            }
        }

        public string LoginbtnStatus => IsLoading ? "Authenticating..." : "Login";

        private async Task ShowErrorAsync(string message)
        {
            ErrorMessage = message;
            await Task.Delay(2000);
            ErrorMessage = null;
        }

        private async Task LoginAuth()
        {
            if (!CanLogin())
                return;

            IsLoading = true;
            try
            {
                Result<UserDto> result = await _authService.LoginAsync(Username, _password);

                if (result.IsSuccess)
                {
                    _session.CurrentUser = result.Value;

                    var openShift = await _shiftService.GetOpenShiftAsync(result.Value!.UserId);
                    if (openShift.IsSuccess)
                    {
                        _session.CurrentShift = openShift.Value;
                    }

                    LoginSucceeded?.Invoke();
                }
                else
                {
                    await ShowErrorAsync(result.Error ?? "Login failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Username}", Username);
                await ShowErrorAsync("An unexpected error occurred. Please try again.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public LoginViewModel(
            IAuthService authService,
            ISessionService session,
            IShiftService shiftService,
            IDialogService dialogService,
            ILocalizationService localizationService,
            ILogger<LoginViewModel> logger)
        {
            _authService = authService;
            _session = session;
            _shiftService = shiftService;
            _dialogService = dialogService;
            _localizationService = localizationService;
            _logger = logger;

            SupportedLanguages = new ObservableCollection<LanguageDto>(
                _localizationService.GetSupportedLanguages());

            _selectedLanguage = SupportedLanguages
                .FirstOrDefault(l => l.Code == _localizationService.CurrentLanguage.Code);

            LoginCommand = new AsyncRelayCommand(LoginAuth, CanLogin);
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(_password);
    }
}
