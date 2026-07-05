using System;
using System.Threading;
using System.Threading.Tasks;

namespace UI.ViewModels
{
    public partial class ManagerMainViewModel
    {
        private void UpdateCurrentDateTime()
        {
            CurrentDateTime = !string.IsNullOrWhiteSpace(CurrentDayName) &&
                              !string.IsNullOrWhiteSpace(CurrentDate) &&
                              !string.IsNullOrWhiteSpace(CurrentTime)
                ? $"{CurrentDayName}, {CurrentDate}   {CurrentTime}"
                : string.Empty;
        }

        private void NavigateToTransactions()
        {
            CurrentPage = _transactionsViewModel;
            _ = _transactionsViewModel.RefreshAsync();
        }

        private void NavigateToShiftManagement()
        {
            CurrentPage = _shiftManagementViewModel;
            _ = _shiftManagementViewModel.RefreshAsync();
        }

        private void NavigateToReports()
        {
            CurrentPage = _reportViewModel;
        }

        private void NavigateToProductManagement()
        {
            CurrentPage = _productManagementViewModel;
        }

        private void NavigateToCategoryManagement()
        {
            CurrentPage = _categoryManagementViewModel;
        }

        private void NavigateToReceiptManagement()
        {
            CurrentPage = _receiptManagementViewModel;
        }

        private void InitializeManagerInfo()
        {
            var currentUser = _sessionService.CurrentUser;
            ManagerName = currentUser?.FullName ?? "Manager";
            CurrentDayName = DateTime.Now.ToString("dddd");
            CurrentDate = DateTime.Now.ToString("MMMM dd, yyyy");
            CurrentTime = DateTime.Now.ToString("hh:mm tt");
            UpdateCurrentDateTime();
            _ = UpdateTimeAsync();
        }

        private async Task UpdateTimeAsync()
        {
            try
            {
                _timeCancellationTokenSource = new CancellationTokenSource();
                while (!_timeCancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), _timeCancellationTokenSource.Token);
                    CurrentTime = DateTime.Now.ToString("hh:mm tt");
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
