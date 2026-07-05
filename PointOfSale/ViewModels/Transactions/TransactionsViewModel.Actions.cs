using Contracts.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;
using UI.Commands;

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

            await LoadTransactions();
            _isLoaded = true;
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

        private void OpenReceipt(TransactionListItemDto? transaction)
        {
            if (transaction == null)
                return;

            _receiptDisplayService.ShowReceipt(transaction.TransactionId);
        }
    }
}
