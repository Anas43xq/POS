using BLL.Interfaces;
using Contracts.Transactions;
using Contracts.Enum;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public partial class TransactionsViewModel : BaseViewModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IReceiptDisplayService _receiptDisplayService;

        private readonly ObservableCollection<TransactionListItemDto> _transactions = new();
        private ICollectionView? _transactionsView;
        private string _errorMessage = string.Empty;
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _totalCount;
        private bool _isLoaded;

        public TransactionsViewModel(
            ITransactionService transactionService,
            IReceiptDisplayService receiptDisplayService)
        {
            _transactionService = transactionService;
            _receiptDisplayService = receiptDisplayService;

            LoadDayCommand = new RelayCommand(_ => LoadDay());
            LoadWeekCommand = new RelayCommand(_ => LoadWeek());
            LoadMonthCommand = new RelayCommand(_ => LoadMonth());
            LoadPeriodCommand = new RelayCommand(_ => LoadPeriod());
            ApplyPeriodCommand = new RelayCommand(_ => ApplyPeriod());
            RefreshCommand = new RelayCommand(_ => _ = RefreshAsync());
            OpenReceiptCommand = new RelayCommand<TransactionListItemDto>(OpenReceipt);
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CanGoPreviousPage);
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CanGoNextPage);
            VoidTransactionCommand = new AsyncRelayCommand<TransactionListItemDto?>(
                VoidTransactionAsync,
                CanVoidTransaction);

            Transactions = _transactions;
            TransactionsView = CollectionViewSource.GetDefaultView(Transactions);
            TransactionsView.Filter = FilterTransactions;

            CurrentFilterMode = TransactionFilterMode.Day;
            // Initialise the status filter from the options list so the
            // combobox shows "Completed" on first render.
            _selectedStatusFilter = StatusOptions[0];
            IsPeriodFilterVisible = false;
        }

        public ObservableCollection<TransactionListItemDto> Transactions { get; }

        public ICollectionView TransactionsView
        {
            get => _transactionsView!;
            private set
            {
                _transactionsView = value;
                OnPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (_errorMessage == value)
                    return;

                _errorMessage = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        private TransactionFilterMode _currentFilterMode;
        public TransactionFilterMode CurrentFilterMode
        {
            get => _currentFilterMode;
            private set
            {
                if (_currentFilterMode == value)
                    return;

                _currentFilterMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPeriodFilterVisible));
            }
        }

        /// <summary>
        /// Status filter options shown in the combobox.  The wrapper class
        /// carries a user-facing label and the underlying nullable enum
        /// (null = "All", no status filter).  Bound to the XAML combobox;
        /// the enum value is passed to the SP via <c>ToString()</c>.
        /// </summary>
        public IReadOnlyList<StatusFilterOption> StatusOptions { get; } = new[]
        {
            new StatusFilterOption("Completed", TransactionStatus.Completed),
            new StatusFilterOption("Voided",    TransactionStatus.Voided),
            new StatusFilterOption("Pending",   TransactionStatus.Pending),
            new StatusFilterOption("All",       null),
        };

        private StatusFilterOption _selectedStatusFilter;
        public StatusFilterOption SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (value == null || _selectedStatusFilter == value)
                    return;

                _selectedStatusFilter = value;

                // Status and date filters compose: changing the status keeps
                // the active date filter intact so the two can be combined
                // (e.g. "this week's Voided transactions").  We just clear
                // any stale error message and reload with the new status.
                ErrorMessage = string.Empty;
                CurrentPage = 1;
                _ = LoadTransactions();
            }
        }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (_fromDate == value)
                    return;

                _fromDate = value;
                OnPropertyChanged();
                if (CurrentFilterMode == TransactionFilterMode.Period)
                    RefreshView();
            }
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (_toDate == value)
                    return;

                _toDate = value;
                OnPropertyChanged();
                if (CurrentFilterMode == TransactionFilterMode.Period)
                    RefreshView();
            }
        }

        private bool _isPeriodFilterVisible;
        public bool IsPeriodFilterVisible
        {
            get => _isPeriodFilterVisible;
            private set
            {
                if (_isPeriodFilterVisible == value)
                    return;

                _isPeriodFilterVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadDayCommand { get; }
        public ICommand LoadWeekCommand { get; }
        public ICommand LoadMonthCommand { get; }
        public ICommand LoadPeriodCommand { get; }
        public ICommand ApplyPeriodCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand OpenReceiptCommand { get; }
        public ICommand VoidTransactionCommand { get; }

        private TransactionListItemDto? _selectedTransaction;
        public TransactionListItemDto? SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                if (_selectedTransaction == value)
                    return;

                _selectedTransaction = value;
                OnPropertyChanged();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            private set
            {
                if (_currentPage == value)
                    return;

                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CanGoPreviousPage));
                OnPropertyChanged(nameof(CanGoNextPage));
                RaisePagingCanExecuteChanged();
            }
        }

        public int PageSize
        {
            get => _pageSize;
            private set
            {
                if (_pageSize == value)
                    return;

                _pageSize = value;
                OnPropertyChanged();
            }
        }

        public int TotalCount
        {
            get => _totalCount;
            private set
            {
                if (_totalCount == value)
                    return;

                _totalCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CanGoNextPage));
                RaisePagingCanExecuteChanged();
            }
        }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public string PageInfo => TotalPages == 0 ? "0 of 0" : $"{CurrentPage} of {TotalPages}";
        public bool CanGoPreviousPage => CurrentPage > 1;
        public bool CanGoNextPage => CurrentPage < TotalPages;

    }

    /// <summary>
    /// A single option in the Status filter combobox.  Wraps a user-facing
    /// label and a nullable <see cref="TransactionStatus"/> (null = "All").
    /// </summary>
    public sealed class StatusFilterOption
    {
        public StatusFilterOption(string label, TransactionStatus? value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; }
        public TransactionStatus? Value { get; }

        public override string ToString() => Label;
    }

    public enum TransactionFilterMode
    {
        Day,
        Week,
        Month,
        Period
    }
}
