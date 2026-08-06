using System;
using System.Threading;
using System.Threading.Tasks;
using UI.Views;

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
            _ = _productManagementViewModel.EnsureDataLoadedAsync();
        }

        private void NavigateToCategoryManagement()
        {
            CurrentPage = _categoryManagementViewModel;
            _ = _categoryManagementViewModel.EnsureDataLoadedAsync();
        }

        private void NavigateToSizeManagement()
        {
            CurrentPage = _sizeManagementViewModel;
            _ = _sizeManagementViewModel.EnsureDataLoadedAsync();
        }

        private void NavigateToReceiptManagement()
        {
            CurrentPage = _receiptManagementViewModel;
        }

        private void NavigateToModifierGroupManagement()
        {
            CurrentPage = _modifierGroupManagementViewModel;
            _ = _modifierGroupManagementViewModel.EnsureDataLoadedAsync();
        }

        private void OnReceiptNavigateToForm()
        {
            _receiptFormPage = new Views.PurchaseReceiptFormView
            {
                DataContext = _receiptManagementViewModel
            };
            CurrentPage = _receiptFormPage;
        }

        private void OnReceiptNavigateToList()
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

        /// <summary>
        /// Opens the same Settings dialog the cashier header uses. The
        /// dialog's ViewModel is created through <see cref="IViewModelFactory"/>
        /// so it picks up the current language and all its services the
        /// same way every other dialog in the app does.
        /// </summary>
        private async Task OpenSetting()
        {
            var vm = _viewModelFactory.Create<SettingsViewModel>();
            _dialogService.ShowDialog<SettingsWindow>(vm);
            await Task.CompletedTask;
        }
    }
}
