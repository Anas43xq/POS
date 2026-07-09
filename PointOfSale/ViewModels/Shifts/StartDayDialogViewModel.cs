using BLL.Interfaces;
using DAL.Entities;
using System;
using System.Windows;
using System.Windows.Input;
using UI.Commands;

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

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand StartDayCommand { get; }
        public ICommand CancelCommand { get; }

        public StartDayDialogViewModel(
            IShiftService shiftService,
            ISessionService sessionService)
        {
            _shiftService = shiftService;
            _sessionService = sessionService;

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
            try
            {
                ErrorMessage = string.Empty;

                // Validate input
                if (_sessionService.CurrentUser == null)
                {
                    ErrorMessage = "User not authenticated.";
                    return;
                }

                if (!decimal.TryParse(OpeningCash, out decimal openingCash))
                {
                    ErrorMessage = "Invalid opening cash amount.";
                    return;
                }

                if (openingCash < 0)
                {
                    ErrorMessage = "Opening cash cannot be negative.";
                    return;
                }

                // Attempt to open shift
                var result = await _shiftService.OpenShiftAsync(
                    _sessionService.CurrentUser.UserId, 
                    openingCash);

                if (result.IsSuccess)
                {
                    _sessionService.CurrentShift = result.Value;
                    CloseDialog();
                }
                else
                {
                    ErrorMessage = result.Error ?? "Failed to start shift.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }
}
