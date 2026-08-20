using Contracts.Transactions;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public partial class TransactionsViewModel
    {
        public async Task LoadTransactions(CancellationToken ct = default)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                if (CurrentFilterMode == TransactionFilterMode.Period)
                {
                    if (!FromDate.HasValue)
                    {
                        ErrorMessage = "From date is required for a custom period.";
                        return;
                    }

                    if (ToDate.HasValue && ToDate.Value < FromDate.Value)
                    {
                        ErrorMessage = "To date cannot be earlier than From date.";
                        return;
                    }
                }

                var request = new GetTransactionsListRequest
                {
                    PeriodType = CurrentFilterMode switch
                    {
                        TransactionFilterMode.Day => "Today",
                        TransactionFilterMode.Week => "Week",
                        TransactionFilterMode.Month => "Month",
                        TransactionFilterMode.Period => "Custom",
                        _ => "Today"
                    },
                    FromDate = FromDate,
                    ToDate = ToDate,
                    StatusFilter = SelectedStatusFilter?.Value?.ToString(),
                    PageNumber = CurrentPage,
                    PageSize = PageSize
                };

                if (request.PeriodType != "Custom")
                {
                    request.FromDate = null;
                    request.ToDate = null;
                }

                var paged = await _transactionService.GetTransactionsListAsync(request);
                TotalCount = paged.TotalCount;
                CurrentPage = paged.PageNumber;
                PageSize = paged.PageSize;

                Transactions.Clear();
                foreach (var transaction in paged.Items)
                {
                    Transactions.Add(transaction);
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadAsync()
        {
            if (_isLoaded)
                return;

            var stopwatch = Stopwatch.StartNew();
            await LoadTransactions();
            _isLoaded = true;
            TxpTrace.WriteLine($"[TXP] - Transactions first load completed in {stopwatch.ElapsedMilliseconds} ms");
        }

        public async Task RefreshAsync()
        {
            await LoadTransactions();
        }

        private void LoadDay()
        {
            CurrentFilterMode = TransactionFilterMode.Day;
            IsPeriodFilterVisible = false;
            CurrentPage = 1;
            _ = LoadTransactions();
        }

        private void LoadWeek()
        {
            CurrentFilterMode = TransactionFilterMode.Week;
            IsPeriodFilterVisible = false;
            CurrentPage = 1;
            _ = LoadTransactions();
        }

        private void LoadMonth()
        {
            CurrentFilterMode = TransactionFilterMode.Month;
            IsPeriodFilterVisible = false;
            CurrentPage = 1;
            _ = LoadTransactions();
        }

        private void LoadPeriod()
        {
            CurrentFilterMode = TransactionFilterMode.Period;
            IsPeriodFilterVisible = true;
        }

        private void ApplyPeriod()
        {
            if (!FromDate.HasValue)
            {
                ErrorMessage = "From date is required for a custom period.";
                return;
            }

            ErrorMessage = string.Empty;
            CurrentFilterMode = TransactionFilterMode.Period;
            IsPeriodFilterVisible = true;
            CurrentPage = 1;
            _ = LoadTransactions();
        }

        private void PreviousPage()
        {
            if (!CanGoPreviousPage)
                return;

            CurrentPage--;
            _ = LoadTransactions();
        }

        private void NextPage()
        {
            if (!CanGoNextPage)
                return;

            CurrentPage++;
            _ = LoadTransactions();
        }

        private void RaisePagingCanExecuteChanged()
        {
            if (PreviousPageCommand is RelayCommand prev)
                prev.RaiseCanExecuteChanged();

            if (NextPageCommand is RelayCommand next)
                next.RaiseCanExecuteChanged();
        }

        private void RefreshView()
        {
            if (TransactionsView == null)
                return;

            TransactionsView.Refresh();
        }

        public bool FilterTransactions(object obj)
        {
            return true;
        }

        private async Task OpenReceipt(TransactionListItemDto? transaction)
        {
            if (transaction == null)
                return;

            await _receiptDisplayService.ShowReceiptAsync(transaction.TransactionId);
        }

        /// <summary>
        /// CanExecute for <see cref="VoidTransactionCommand"/>: only fire
        /// for a non-null row whose current status is "Completed".
        /// </summary>
        private bool CanVoidTransaction(TransactionListItemDto? transaction)
        {
            return transaction != null
                && string.Equals(transaction.Status, "Completed", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Voids the supplied transaction.  The PIN overlay acts as both
        /// confirmation and authorization. An optional reason is persisted
        /// when the manager provides one.
        /// </summary>
        private async Task VoidTransactionAsync(TransactionListItemDto? transaction)
        {
            if (transaction == null) return;

            // PIN overlay acts as confirmation + authorization.
            var voidReason = await _managerOverlayService.RequestApprovalWithReasonAsync(
                _localizationService.GetString("Void.ApprovalTitle"));

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var result = await _transactionService.VoidTransactionAsync(
                    transaction.TransactionId,
                    voidReason);

                SelectedTransaction = null;

                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error
                        ?? _localizationService.GetString("Transactions.VoidFailed");
                    await RefreshAsync();
                    return;
                }

                var receiptNumber = result.Value?.ReceiptNumber ?? transaction.ReceiptNumber;
                _notifications.ShowSuccess(
                    $"Transaction {receiptNumber} voided successfully.");

                await RefreshAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
