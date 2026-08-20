using BLL.Interfaces;
using DAL.Entities;
using System;
using System.Windows;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Start Day dialog.
    /// Handles opening cash input and shift creation.
    /// Pure MVVM - no events or code-behind required.
    /// </summary>
    public class StartDayDialogViewModel : BaseViewModel
    {
        private readonly IShiftService _shiftService;
        private readonly ISessionService _sessionService;
        private readonly ILocalizationService _localizationService;

        private string _openingCash = "";
        public string OpeningCash
        {
            get => _openingCash;
            set
            {
                _openingCash = value ?? "";
                OnPropertyChanged();
            }
        }

        public ICommand StartDayCommand { get; }
        public ICommand CancelCommand { get; }

        public StartDayDialogViewModel(
            IShiftService shiftService,
            ISessionService sessionService,
            INotificationService notifications,
            ILocalizationService localizationService)
        {
            _shiftService = shiftService;
            _sessionService = sessionService;
            Notifications = notifications;
            _localizationService = localizationService;

            StartDayCommand = new AsyncRelayCommand(StartDayAsync, CanStartDay);
            CancelCommand = new RelayCommand(CloseDialog);
        }

        private bool CanStartDay()
        {
            return _sessionService.CurrentUser != null && 
                   decimal.TryParse(OpeningCash, out _);
        }

        private void CloseDialog()
        {
            // Close the dialog by requesting the window to close
            // This will be handled by the DialogService
            Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }

        private async Task StartDayAsync()
        {
            // Validate input
            if (_sessionService.CurrentUser == null)
            {
                Notifications?.ShowError(_localizationService.GetString("Shift.UserNotAuthenticated"));
                return;
            }

            if (!decimal.TryParse(OpeningCash, out decimal openingCash))
            {
                Notifications?.ShowError(_localizationService.GetString("Shift.InvalidOpeningCash"));
                return;
            }

            // Attempt to open shift. ShiftService.OpenShiftAsync validates
            // that openingCash is non-negative and returns Result.Failure
            // with the appropriate message; errors are surfaced via
            // BaseViewModel.RunAsync.
            await RunAsync(
                () => _shiftService.OpenShiftAsync(
                    _sessionService.CurrentUser.UserId,
                    openingCash),
                async shift =>
                {
                    _sessionService.CurrentShift = shift;
                    CloseDialog();
                });
        }
    }
}