using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UI.Views;
using UI.Services;

namespace UI.ViewModels
{
    public partial class ManagerMainViewModel
    {
        private void ActivatePage(object? page, string pageName, Action? afterActivation = null)
        {
            if (ReferenceEquals(CurrentPage, page))
                return;

            var stopwatch = Stopwatch.StartNew();
            CurrentPage = page;
            afterActivation?.Invoke();
            TxpTrace.WriteLine($"[TXP] - Manager sidebar activated {pageName} in {stopwatch.ElapsedMilliseconds} ms");
        }

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
            ActivatePage(_transactionsViewModel, nameof(TransactionsViewModel), () => _ = _transactionsViewModel.LoadAsync());
        }

        private void NavigateHome()
        {
            ActivatePage(_homeViewModel, nameof(HomeViewModel), () => _ = _homeViewModel.EnsureDataLoadedAsync());
        }

        private void NavigateToShiftManagement()
        {
            ActivatePage(_shiftManagementViewModel, nameof(ShiftManagementViewModel), () => _ = _shiftManagementViewModel.LoadAsync());
        }

        private void NavigateToReports()
        {
            ActivatePage(_reportViewModel, nameof(ReportViewModel));
        }

        private void NavigateToProductManagement()
        {
            ActivatePage(_productManagementViewModel, nameof(ProductManagementViewModel), () => _ = _productManagementViewModel.EnsureDataLoadedAsync());
        }

        private void NavigateToCategoryManagement()
        {
            ActivatePage(_categoryManagementViewModel, nameof(CategoryManagementViewModel), () => _ = _categoryManagementViewModel.EnsureDataLoadedAsync());
        }

        private void NavigateToSizeManagement()
        {
            ActivatePage(_sizeManagementViewModel, nameof(SizeManagementViewModel), () => _ = _sizeManagementViewModel.EnsureDataLoadedAsync());
        }

        private void NavigateToReceiptManagement()
        {
            ActivatePage(_receiptManagementViewModel, nameof(ReceiptManagementViewModel), () => _ = _receiptManagementViewModel.EnsureDataLoadedAsync());
        }

        private void NavigateToModifierGroupManagement()
        {
            ActivatePage(_modifierGroupManagementViewModel, nameof(ModifierGroupManagementViewModel), () => _ = _modifierGroupManagementViewModel.EnsureDataLoadedAsync());
        }

        private void OnReceiptNavigateToForm()
        {
            // Reuse a single form View instance instead of constructing a
            // brand-new one (InitializeComponent + full visual tree) every
            // time the user opens Add/Edit — this avoided repeated,
            // avoidable UI-thread work on each round trip.
            _receiptFormPage ??= new Views.PurchaseReceiptFormView
            {
                DataContext = _receiptManagementViewModel
            };
            ActivatePage(_receiptFormPage, nameof(PurchaseReceiptFormView));
        }

        private void OnReceiptNavigateToList()
        {
            ActivatePage(_receiptManagementViewModel, nameof(ReceiptManagementViewModel));
        }

        private void OnReceiptCloseRequested()
        {
            ActivatePage(_homeViewModel, nameof(HomeViewModel));
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
