using BLL.Interfaces;
using Contracts.Transactions;
using DAL.Entities;
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

            Transactions = _transactions;
            TransactionsView = CollectionViewSource.GetDefaultView(Transactions);
            TransactionsView.Filter = FilterTransactions;

            CurrentFilterMode = TransactionFilterMode.Day;
            SelectedStatusFilter = "Completed";
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

        private string _selectedStatusFilter = string.Empty;
        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                var normalized = value ?? string.Empty;
                if (string.Equals(_selectedStatusFilter, normalized, StringComparison.Ordinal))
                    return;

                _selectedStatusFilter = normalized;
                OnPropertyChanged();
                RefreshView();
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
                OnPropertyChanged(nameof(PageInfo));
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

    public enum TransactionFilterMode
    {
        Day,
        Week,
        Month,
        Period
    }
}
