using BLL.Interfaces;
using DAL.Entities;
using System;
using System.Windows;
using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels
{
    /// <summary>
    /// ViewModel for the End Day dialog.
    /// Handles closing cash input, displays shift summary with cash reconciliation.
    /// Pure MVVM - no events or code-behind required.
    /// </summary>
    public class EndDayDialogViewModel : BaseViewModel
    {
        private readonly IShiftService _shiftService;
        private readonly ISessionService _sessionService;

        private Shift? _currentShift;

        private string _closingCash;
        public string ClosingCash
        {
            get => _closingCash;
            set
            {
                _closingCash = value ?? "0.00";
                OnPropertyChanged();
                UpdateCashDifference();
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

        private bool _showCashDifference = false;
        public bool ShowCashDifference
        {
            get => _showCashDifference;
            set
            {
                _showCashDifference = value;
                OnPropertyChanged();
            }
        }

        public string OpeningCashDisplay => _currentShift?.OpeningCash.ToString("C2") ?? "N/A";
        public string TotalSalesDisplay => "0.00"; // Will be calculated from transactions
        public string ExpectedCashDisplay => (_currentShift?.OpeningCash ?? 0).ToString("C2");

        private decimal _cashDifference = 0;
        public string CashDifferenceDisplay
        {
            get
            {
                var color = _cashDifference >= 0 ? "+" : "";
                return $"{color}{_cashDifference:C2}";
            }
        }

        public ICommand EndDayCommand { get; }
        public ICommand CancelCommand { get; }

        public EndDayDialogViewModel(
            IShiftService shiftService,
            ISessionService sessionService)
        {
            _shiftService = shiftService;
            _sessionService = sessionService;
            _currentShift = sessionService.CurrentShift;
            _closingCash = string.Empty;

            EndDayCommand = new AsyncRelayCommand(EndDayAsync, CanEndDay);
            CancelCommand = new RelayCommand(CloseDialog);
        }

        private bool CanEndDay()
        {
            return _currentShift != null && 
                   _currentShift.ShiftId > 0 &&
                   decimal.TryParse(ClosingCash, out _);
        }

        private void CloseDialog()
        {
            // Close the dialog by requesting the window to close
            // This will be handled by the DialogService
            Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }

        private void UpdateCashDifference()
        {
            if (decimal.TryParse(ClosingCash, out decimal closingCash))
            {
                _cashDifference = closingCash - (_currentShift?.OpeningCash ?? 0);
                ShowCashDifference = true;
                OnPropertyChanged(nameof(CashDifferenceDisplay));
            }
            else
            {
                ShowCashDifference = false;
            }
        }

        private async Task EndDayAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                // Validate current shift
                if (_currentShift == null || _currentShift.ShiftId <= 0)
                {
                    ErrorMessage = "No active shift found.";
                    return;
                }

                // Validate input
                if (!decimal.TryParse(ClosingCash, out decimal closingCash))
                {
                    ErrorMessage = "Invalid closing cash amount.";
                    return;
                }

                if (closingCash < 0)
                {
                    ErrorMessage = "Closing cash cannot be negative.";
                    return;
                }

                // Attempt to close shift
                var result = await _shiftService.CloseShiftAsync(
                    _currentShift.ShiftId,
                    closingCash);

                if (result.IsSuccess)
                {
                    _sessionService.CurrentShift = null;
                    CloseDialog();
                }
                else
                {
                    ErrorMessage = result.Error ?? "Failed to close shift.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }
}
